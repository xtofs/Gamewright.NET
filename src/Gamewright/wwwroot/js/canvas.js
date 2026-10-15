// Host side of the immediate-mode loop.
// JS owns: the requestAnimationFrame loop, input capture, canvas sizing,
// and replaying the command buffer .NET submits once per frame.
// .NET owns: all application state and deciding what to draw each frame.

let canvas,
  ctx,
  frameCb,
  running = false;

// Input block copied into .NET once per frame (layout mirrors Input.cs).
// 0 width(css px) 1 height(css px) 2 devicePixelRatio
// 3 pointerX 4 pointerY 5 buttonsDown 6 pressedThisFrame 7 releasedThisFrame
const input = new Float32Array(8);
const inputBytes = new Uint8Array(input.buffer); // the MemoryView from .NET is a byte view

const strings = []; // string table, filled lazily via defineString

const images = []; // id -> ImageBitmap, filled by loadImage
const loading = new Set(); // ids whose load is still in flight
let imageLoadedCb; // .NET callback (id, width, height); width 0 means the load failed

export function init(canvasId, frame, imageLoaded, opCodes, pathVerbs) {
  defineTables(opCodes, pathVerbs);
  canvas = document.getElementById(canvasId);
  ctx = canvas.getContext("2d");
  frameCb = frame;
  imageLoadedCb = imageLoaded;

  canvas.addEventListener("pointermove", (e) => {
    input[3] = e.offsetX;
    input[4] = e.offsetY;
  });
  canvas.addEventListener("pointerdown", (e) => {
    input[3] = e.offsetX;
    input[4] = e.offsetY;
    input[5] = e.buttons;
    input[6] |= buttonBit(e.button);
    canvas.setPointerCapture(e.pointerId);
  });
  canvas.addEventListener("pointerup", (e) => {
    input[5] = e.buttons;
    input[7] |= buttonBit(e.button);
  });
  canvas.addEventListener("contextmenu", (e) => e.preventDefault());
  canvas.addEventListener("keydown", function (event) {
    console.log("Key pressed:", event.key, "code:", event.code);
  });

  running = true;
  requestAnimationFrame(tick);
}

// e.button (0 primary, 1 middle, 2 secondary) -> the e.buttons bitmask convention,
// so ButtonsDown, Pressed and Released all use the same bits.
const buttonBit = (b) => [1, 4, 2, 8, 16][b] ?? 0;

export function stop() {
  running = false;
}

export function defineString(id, value) {
  strings[id] = value;
}

// Fetch + decode off the main thread; the bitmap draws fast and is released by freeImage.
export async function loadImage(id, url) {
  loading.add(id);
  let bitmap;
  try {
    const response = await fetch(url);
    if (!response.ok) throw new Error(`HTTP ${response.status}`);
    bitmap = await createImageBitmap(await response.blob());
  } catch (e) {
    console.warn(`Gamewright: could not load image '${url}':`, e);
  }
  if (!loading.delete(id)) {
    bitmap?.close(); // freed while it was still loading
    return;
  }
  images[id] = bitmap;
  imageLoadedCb(id, bitmap ? bitmap.width : 0, bitmap ? bitmap.height : 0);
}

export function freeImage(id) {
  loading.delete(id);
  images[id]?.close();
  images[id] = undefined;
}

// Called from .NET inside the frame callback. The view is only valid during the call.
export function readInput(view) {
  view.set(inputBytes);
  input[6] = 0;
  input[7] = 0; // edge-triggered bits are per frame
}

function tick(t) {
  if (!running) return;
  requestAnimationFrame(tick); // schedule first so an exception doesn't kill the loop
  resize();
  frameCb(t); // .NET: read input, build commands, submit
}

function resize() {
  const dpr = window.devicePixelRatio || 1;
  const w = canvas.clientWidth;
  const h = canvas.clientHeight;

  const pw = Math.round(w * dpr),
    ph = Math.round(h * dpr);
  if (canvas.width !== pw || canvas.height !== ph) {
    canvas.width = pw;
    canvas.height = ph;
  }
  ctx.setTransform(dpr, 0, 0, dpr, 0, 0); // .NET draws in CSS pixels
  input[0] = w;
  input[1] = h;
  input[2] = dpr;
}

// Colors are packed 0xRRGGBBAA and travel as raw uint32 bits (read via the Uint32Array view).
function cssColor(v) {
  const r = (v >>> 24) & 255;
  const g = (v >>> 16) & 255;
  const b = (v >>> 8) & 255;
  const a = (v & 255) / 255;
  return `rgba(${r} ${g} ${b} / ${a})`;
}

// Opcode and path verb numbers belong to the OpCode and PathVerb enums in .NET. .NET hands them over in init, and
// init checks that this file and .NET agree on the set of ops and verbs and on how many operands each verb has,
// so a mismatch fails at startup instead of mid-frame.
const OpCode = {};
const HANDLED_OPS = ["Clear", "FillPath", "StrokePath", "DrawText", "DrawImage"];

const PathVerb = {};
// verb -> number of operand slots that follow it
const HANDLED_VERBS = {
  MoveTo: 2,
  LineTo: 2,
  QuadraticCurveTo: 4,
  BezierCurveTo: 6,
  Ellipse: 8,
  Close: 0,
};

// "Clear=0,FillPath=1,…" and "MoveTo=0:2,…" -> OpCode and PathVerb.
function defineTables(opCodes, pathVerbs) {
  const ops = parseTable(opCodes);
  const verbs = parseTable(pathVerbs);

  const problems = [
    ...compare("op", Object.keys(ops), HANDLED_OPS),
    ...compare("path verb", Object.keys(verbs), Object.keys(HANDLED_VERBS)),
    ...Object.keys(verbs)
      .filter((n) => n in HANDLED_VERBS && verbs[n].operands !== HANDLED_VERBS[n])
      .map(
        (n) =>
          `path verb ${n} has ${verbs[n].operands} operands in .NET but ${HANDLED_VERBS[n]} in canvas.js`,
      ),
  ];
  if (problems.length) {
    throw new Error(`Gamewright: canvas.js and .NET disagree. ${problems.join("; ")}.`);
  }

  for (const [name, { value }] of Object.entries(ops)) OpCode[name] = value;
  for (const [name, { value }] of Object.entries(verbs)) PathVerb[name] = value;
}

// "Name=Number[:Operands],…" -> { Name: { value, operands } }
function parseTable(text) {
  const table = {};
  for (const pair of text.split(",")) {
    const [name, rest] = pair.split("=");
    const [value, operands] = rest.split(":");
    table[name] = { value: Number(value), operands: operands === undefined ? undefined : Number(operands) };
  }
  return table;
}

function compare(kind, fromDotnet, handled) {
  const problems = [];
  const notHandled = fromDotnet.filter((n) => !handled.includes(n));
  const notDefined = handled.filter((n) => !fromDotnet.includes(n));
  if (notHandled.length) problems.push(`${kind}s not handled by canvas.js: [${notHandled}]`);
  if (notDefined.length) problems.push(`${kind}s not defined by .NET: [${notDefined}]`);
  return problems;
}

// Reads "n slot…" starting at f[i]: n slots of path verbs, each followed by its operands. Builds the path on ctx
// and returns the index just past it, so the caller continues with the fixed tail (line width, color).
function buildPath(f, i) {
  const end = i + 1 + f[i];
  i++;
  ctx.beginPath();
  while (i < end) {
    switch (f[i++]) {
      case PathVerb.MoveTo:
        ctx.moveTo(f[i], f[i + 1]);
        i += 2;
        break;
      case PathVerb.LineTo:
        ctx.lineTo(f[i], f[i + 1]);
        i += 2;
        break;
      case PathVerb.QuadraticCurveTo:
        ctx.quadraticCurveTo(f[i], f[i + 1], f[i + 2], f[i + 3]);
        i += 4;
        break;
      case PathVerb.BezierCurveTo:
        ctx.bezierCurveTo(f[i], f[i + 1], f[i + 2], f[i + 3], f[i + 4], f[i + 5]);
        i += 6;
        break;
      case PathVerb.Ellipse:
        // cx cy rx ry rotation startAngle endAngle counterClockwise
        ctx.ellipse(f[i], f[i + 1], f[i + 2], f[i + 3], f[i + 4], f[i + 5], f[i + 6], f[i + 7] !== 0);
        i += 8;
        break;
      case PathVerb.Close:
        ctx.closePath();
        break;
      default:
        throw new Error(`Unknown path verb ${f[i - 1]} at ${i - 1}`);
    }
  }
  if (i !== end) throw new Error(`Path overran its ${end - i} slots at ${i}`);
  return end;
}

export function submit(view) {
  const bytes = view.slice(); // copy out of WASM memory (view dies after this call)
  const f = new Float32Array(
    bytes.buffer,
    bytes.byteOffset,
    bytes.byteLength / 4,
  ); // opcodes, numbers
  const u = new Uint32Array(
    bytes.buffer,
    bytes.byteOffset,
    bytes.byteLength / 4,
  ); // colors (raw bits)
  let i = 0;
  while (i < f.length) {
    switch (f[i++]) {
      case OpCode.Clear: {
        // Clear rgba
        ctx.fillStyle = cssColor(u[i++]);
        ctx.fillRect(0, 0, canvas.clientWidth, canvas.clientHeight);
        break;
      }
      case OpCode.FillPath: {
        // FillPath n slot… rgba
        i = buildPath(f, i);
        ctx.fillStyle = cssColor(u[i++]);
        ctx.fill();
        break;
      }
      case OpCode.StrokePath: {
        // StrokePath n slot… lineWidth rgba
        i = buildPath(f, i);
        ctx.lineWidth = f[i++];
        ctx.strokeStyle = cssColor(u[i++]);
        ctx.stroke();
        break;
      }
      case OpCode.DrawText: {
        // Text strId x y size rgba (centered)
        const s = strings[f[i++]],
          x = f[i++],
          y = f[i++],
          size = f[i++];
        ctx.fillStyle = cssColor(u[i++]);
        ctx.font = `${size}px system-ui, sans-serif`;
        ctx.textAlign = "center";
        ctx.textBaseline = "middle";
        // https://developer.mozilla.org/en-US/docs/Web/API/CanvasRenderingContext2D/fillText
        ctx.fillText(s, x, y);
        break;
      }
      case OpCode.DrawImage: {
        // Image id sx sy sw sh dx dy dw dh alpha flags (flags bit 0 = pixelated)
        const bitmap = images[f[i++]],
          sx = f[i++],
          sy = f[i++],
          sw = f[i++],
          sh = f[i++],
          dx = f[i++],
          dy = f[i++],
          dw = f[i++],
          dh = f[i++],
          alpha = f[i++],
          flags = f[i++];
        // All operands are consumed above even if the bitmap is gone, so the stream never desyncs.
        if (bitmap) {
          ctx.globalAlpha = alpha;
          ctx.imageSmoothingEnabled = (flags & 1) === 0;
          ctx.drawImage(bitmap, sx, sy, sw, sh, dx, dy, dw, dh);
          ctx.globalAlpha = 1;
          ctx.imageSmoothingEnabled = true;
        }
        break;
      }
      default:
        console.log(`Unknown opcode ${f[i - 1]} at ${i - 1}`);
        throw new Error(`Unknown opcode ${f[i - 1]} at ${i - 1}`);
    }
  }
}
