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

export function init(canvasId, frame) {
  canvas = document.getElementById(canvasId);
  ctx = canvas.getContext("2d");
  frameCb = frame;

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
  const w = canvas.clientWidth,
    h = canvas.clientHeight;
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
function css(v) {
  return `rgba(${v >>> 24},${(v >>> 16) & 255},${(v >>> 8) & 255},${(v & 255) / 255})`;
}

// Opcodes must match DrawList.cs.
export function submit(view) {
  const bytes = view.slice(); // copy out of WASM memory (view dies after this call)
  const c = new Float32Array(bytes.buffer, bytes.byteOffset, bytes.byteLength / 4); // opcodes, numbers
  const u = new Uint32Array(bytes.buffer, bytes.byteOffset, bytes.byteLength / 4); // colors (raw bits)
  let i = 0;
  while (i < c.length) {
    switch (c[i++]) {
      case 0: {
        // Clear rgba
        ctx.fillStyle = css(u[i++]);
        ctx.fillRect(0, 0, canvas.clientWidth, canvas.clientHeight);
        break;
      }
      case 1: {
        // Rect x y w h rgba
        const x = c[i++],
          y = c[i++],
          w = c[i++],
          h = c[i++];
        ctx.fillStyle = css(u[i++]);
        ctx.fillRect(x, y, w, h);
        break;
      }
      case 2: {
        // Circle x y r rgba
        const x = c[i++],
          y = c[i++],
          r = c[i++];
        ctx.fillStyle = css(u[i++]);
        ctx.beginPath();
        ctx.arc(x, y, r, 0, Math.PI * 2);
        ctx.fill();
        break;
      }
      case 3: {
        // Text strId x y size rgba (centered)
        const s = strings[c[i++]],
          x = c[i++],
          y = c[i++],
          size = c[i++];
        ctx.fillStyle = css(u[i++]);
        ctx.font = `${size}px system-ui, sans-serif`;
        ctx.textAlign = "center";
        ctx.textBaseline = "middle";
        ctx.fillText(s, x, y);
        break;
      }
      case 4: {
        // StrokeRect x y w h lineWidth rgba
        const x = c[i++],
          y = c[i++],
          w = c[i++],
          h = c[i++];
        ctx.lineWidth = c[i++];
        ctx.strokeStyle = css(u[i++]);
        ctx.strokeRect(x, y, w, h);
        break;
      }
      default:
        throw new Error(`Unknown opcode ${c[i - 1]} at ${i - 1}`);
    }
  }
}
