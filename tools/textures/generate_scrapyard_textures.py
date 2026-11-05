"""Procedural, seamless (tileable) scrapyard textures for Afterrust.

All noise is built in the frequency domain or with wrap-around distances, so every
texture tiles perfectly. Output: <name>_albedo.jpg and <name>_normal.jpg (OpenGL / Unity
convention, green = up) in the given folder.

    python3 generate_scrapyard_textures.py <output_dir> [size]
"""
import sys
import numpy as np
from PIL import Image

OUT = sys.argv[1] if len(sys.argv) > 1 else "."
N = int(sys.argv[2]) if len(sys.argv) > 2 else 1024
rng = np.random.default_rng(20261105)


def fbm(beta=2.0, aniso=(1.0, 1.0), lo=1.0, n=N, highpass=0.0):
    """Seamless fractal noise in [0,1]; beta = spectral falloff, aniso stretches axes,
    highpass removes features larger than 1/highpass of the tile (hides repetition)."""
    fy = np.fft.fftfreq(n)[:, None] * n * aniso[1]
    fx = np.fft.fftfreq(n)[None, :] * n * aniso[0]
    f = np.sqrt(fx * fx + fy * fy)
    f[0, 0] = 1.0
    amp = 1.0 / np.maximum(f, lo) ** (beta / 2.0)
    amp[0, 0] = 0.0
    amp[f < highpass] = 0.0
    spec = amp * np.exp(2j * np.pi * rng.random((n, n)))
    h = np.real(np.fft.ifft2(spec))
    h -= h.min()
    return h / h.max()


def worley(cells, n=N):
    """Seamless Worley F1 / F2 and cell id (wrap-around distances)."""
    pts = rng.random((cells, cells, 2))
    ids = rng.random((cells, cells))
    y, x = np.mgrid[0:n, 0:n] / n * cells
    cy, cx = np.floor(y).astype(int), np.floor(x).astype(int)
    f1 = np.full((n, n), 9.0)
    f2 = np.full((n, n), 9.0)
    cid = np.zeros((n, n))
    for dy in (-1, 0, 1):
        for dx in (-1, 0, 1):
            gy, gx = cy + dy, cx + dx
            p = pts[gy % cells, gx % cells]
            d = np.hypot(gx + p[..., 0] - x, gy + p[..., 1] - y)
            closer = d < f1
            f2 = np.where(closer, f1, np.minimum(f2, d))
            cid = np.where(closer, ids[gy % cells, gx % cells], cid)
            f1 = np.where(closer, d, f1)
    return f1, f2, cid


def lerp(a, b, t):
    t = np.clip(t, 0, 1)[..., None]
    return a * (1 - t) + b * t


def smooth(e0, e1, x):
    t = np.clip((x - e0) / (e1 - e0), 0, 1)
    return t * t * (3 - 2 * t)


def col(*rgb):
    return np.array(rgb, dtype=float) / 255.0


def save(name, albedo, height, strength):
    img = (np.clip(albedo, 0, 1) * 255).astype(np.uint8)
    Image.fromarray(img).save(f"{OUT}/{name}_albedo.jpg", quality=90)
    # Periodic central differences -> seamless normal map (OpenGL: +Y up, image rows go down).
    du = (np.roll(height, -1, 1) - np.roll(height, 1, 1)) * 0.5
    dv = -(np.roll(height, -1, 0) - np.roll(height, 1, 0)) * 0.5
    nx, ny, nz = -du * strength, -dv * strength, np.ones_like(height)
    ln = np.sqrt(nx * nx + ny * ny + nz * nz)
    nrm = np.stack([nx / ln, ny / ln, nz / ln], -1)
    Image.fromarray(((nrm * 0.5 + 0.5) * 255).astype(np.uint8)).save(f"{OUT}/{name}_normal.jpg", quality=95)
    print("saved", name)


def ground_dirt():
    """Ground tile: compacted dirt with gravel. No large blotches, so the repeat stays invisible."""
    base = fbm(1.8, highpass=4.0)
    fine = fbm(1.2)
    f1, f2, cid = worley(70)
    stones = smooth(0.55, 0.25, f1) * (cid > 0.35)
    c = lerp(col(96, 82, 66), col(120, 106, 88), base)
    c = lerp(c, col(72, 64, 54), fine * 0.5)
    stone_c = lerp(col(98, 94, 88), col(132, 128, 120), cid)
    c = lerp(c, stone_c, stones * 0.7)
    h = base * 0.5 + stones * 1.2 + fine * 0.3
    save("ground_dirt", c, h, 6.0)


def concrete_slab():
    """4 m floor slab: stained concrete, oil spots, joint grooves on the tile border."""
    base = fbm(2.0)
    speck = fbm(0.6)
    stains = smooth(0.6, 0.85, fbm(2.8))
    oil = smooth(0.78, 0.9, fbm(3.2)) * smooth(0.5, 0.7, fbm(2.5))
    c = lerp(col(128, 126, 120), col(160, 157, 150), base)
    c = c + (speck[..., None] - 0.5) * 0.08
    c = c * (1 - 0.18 * stains[..., None])
    c = lerp(c, col(48, 44, 40), oil * 0.75)
    y, x = np.mgrid[0:N, 0:N]
    edge = np.minimum(np.minimum(x, N - 1 - x), np.minimum(y, N - 1 - y))
    groove = smooth(5, 1, edge)
    c = c * (1 - 0.45 * groove[..., None])
    pits = smooth(0.92, 0.97, speck)
    h = base * 0.3 + speck * 0.2 - groove * 2.0 - pits * 0.6
    save("concrete_slab", c, h, 5.0)


def concrete_wall():
    """4 m wall: formwork panel lines every 1 m, tie holes, vertical rain streaks."""
    base = fbm(2.0)
    streak = fbm(1.8, aniso=(0.08, 1.0))
    c = lerp(col(118, 118, 114), col(150, 149, 144), base)
    c = c * (1 - 0.22 * smooth(0.5, 0.9, streak)[..., None])
    y, x = np.mgrid[0:N, 0:N]
    per = N // 4
    line = smooth(4, 0, np.minimum(y % per, per - y % per)) * 0.7 + smooth(3, 0, np.minimum(x % (per * 2), per * 2 - x % (per * 2))) * 0.5
    hy, hx = (y % per) - per // 2, (x % per) - per // 2
    holes = smooth(9, 5, np.hypot(hx, hy))
    c = c * (1 - 0.25 * line[..., None]) * (1 - 0.6 * holes[..., None])
    h = base * 0.4 - line * 1.0 - holes * 2.0
    save("concrete_wall", c, h, 4.0)


def corrugated_rust():
    """2 m sheet: faded paint with rust patches and drips (corrugation is real geometry)."""
    paint = fbm(1.6)
    patches = fbm(2.6)
    drips = fbm(1.8, aniso=(0.06, 1.0))
    rust_mask = np.clip(smooth(0.6, 0.78, patches) + smooth(0.66, 0.82, drips) * 0.7, 0, 1)
    rust_tone = fbm(1.4)
    paint_c = lerp(col(88, 104, 100), col(120, 132, 126), paint)
    rust_c = lerp(col(84, 50, 32), col(128, 76, 46), rust_tone)
    rust_c = lerp(rust_c, col(60, 34, 22), smooth(0.7, 0.95, rust_tone))
    c = lerp(paint_c, rust_c, rust_mask)
    h = rust_mask * 0.8 + rust_tone * rust_mask * 0.6 + paint * 0.1
    save("corrugated_rust", c, h, 4.0)


def rusty_steel():
    """2 m: dark scaled steel with rust, for posts and frames."""
    a = fbm(2.0)
    b = fbm(1.2)
    rust = smooth(0.45, 0.7, fbm(2.4))
    c = lerp(col(52, 50, 48), col(78, 74, 70), a)
    c = lerp(c, lerp(col(92, 54, 34), col(124, 78, 50), b), rust * 0.85)
    h = rust * 0.8 + b * 0.3
    save("rusty_steel", c, h, 4.0)


if __name__ == "__main__":
    ground_dirt()
    concrete_slab()
    concrete_wall()
    corrugated_rust()
    rusty_steel()
