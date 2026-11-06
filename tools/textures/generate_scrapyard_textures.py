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


def concrete_block():
    """2 m tile: cinder block wall (40x20 cm, running bond) with peeling pale paint and grime."""
    y, x = np.mgrid[0:N, 0:N]
    bh = N // 10                      # 10 rows of 20 cm
    bw = N // 5                       # 5 blocks of 40 cm
    row = y // bh
    xs = (x + (row % 2) * (bw // 2)) % N
    my = np.minimum(y % bh, bh - 1 - y % bh)
    mx = np.minimum(xs % bw, bw - 1 - xs % bw)
    mortar = smooth(4, 1, np.minimum(my, mx))
    block_id = (row * 7 + xs // bw * 13) % 17 / 17.0
    pores = fbm(0.5)
    base = fbm(2.0)
    paint_mask = smooth(0.30, 0.36, fbm(2.6, highpass=1.5)) * (1 - mortar * 0.6)
    grime = smooth(0.5, 0.9, fbm(1.8, aniso=(0.06, 1.0)))
    block_c = lerp(col(112, 110, 104), col(140, 137, 130), base * 0.7 + block_id * 0.3)
    block_c = block_c * (0.9 + 0.2 * pores[..., None])
    paint_c = lerp(col(176, 178, 160), col(196, 196, 180), base)
    c = lerp(block_c, paint_c, paint_mask)
    c = c * (1 - 0.35 * mortar[..., None])
    c = c * (1 - 0.3 * grime[..., None])
    edge = np.abs(np.roll(paint_mask, 1, 0) - paint_mask) + np.abs(np.roll(paint_mask, 1, 1) - paint_mask)
    h = -mortar * 1.5 + pores * 0.4 + paint_mask * 0.3 + base * 0.2
    save("concrete_block", c * (1 - 0.4 * np.clip(edge * 4, 0, 1))[..., None], h, 5.0)


def wood_planks():
    """1 m tile: worn planks 20 cm wide, grain along V."""
    y, x = np.mgrid[0:N, 0:N]
    pw = N // 5
    plank = x // pw
    grain = fbm(1.6, aniso=(1.0, 0.05))
    rings = 0.5 + 0.5 * np.sin((grain * 90 + plank * 3.1) * np.pi)
    tone = (plank * 37 % 11) / 11.0
    seam = smooth(3, 0, np.minimum(x % pw, pw - 1 - x % pw))
    wear = smooth(0.55, 0.8, fbm(2.2))
    c = lerp(col(92, 66, 44), col(138, 104, 70), 0.55 * rings + 0.25 * tone + 0.2 * grain)
    c = lerp(c, col(120, 112, 100), wear * 0.45)      # sun-bleached grey
    c = c * (1 - 0.6 * seam[..., None])
    h = rings * 0.3 + grain * 0.5 - seam * 2.0
    save("wood_planks", c, h, 4.0)


def rubber_tire():
    """Tire: U around the wheel (1 = full turn), V across the tread; tread blocks + scuffs."""
    y, x = np.mgrid[0:N, 0:N]
    blocks = 48
    bx = (x * blocks // N) % 2
    across = np.abs(y / N - 0.5) * 2
    tread_zone = smooth(0.62, 0.55, across)
    groove = tread_zone * ((np.minimum(x % (N // blocks), N // blocks - 1 - x % (N // blocks)) < 2) |
                           (np.abs(across - 0.25) < 0.02)).astype(float)
    dust = smooth(0.55, 0.85, fbm(2.0))
    c = lerp(col(24, 24, 24), col(36, 35, 34), fbm(0.8))
    c = lerp(c, col(78, 70, 60), dust * 0.5)
    c = c * (1 - 0.5 * groove[..., None])
    h = -groove * 2.0 + bx * tread_zone * 0.2 + fbm(0.8) * 0.2
    save("rubber_tire", c, h, 6.0)


def chipped_paint():
    """1 m tile, near-white paint (tint with material color) chipped to rust, light scratches."""
    chips = smooth(0.62, 0.66, fbm(1.4, highpass=3.0))
    scratches = smooth(0.8, 0.95, fbm(1.2, aniso=(1.0, 0.03)))
    grime = fbm(2.2)
    paint = lerp(col(205, 205, 200), col(228, 228, 222), grime)
    rust = lerp(col(92, 52, 32), col(130, 76, 44), fbm(1.4))
    c = lerp(paint, rust, np.clip(chips + scratches * 0.5, 0, 1))
    c = c * (1 - 0.18 * smooth(0.5, 0.9, grime)[..., None])
    h = (1 - chips) * 0.6 - scratches * 0.3 + grime * 0.1
    save("chipped_paint", c, h, 4.0)


def pegboard():
    """1 m tile hardboard with 2.5 cm hole grid."""
    y, x = np.mgrid[0:N, 0:N]
    step = N // 40
    hx, hy = (x % step) - step / 2, (y % step) - step / 2
    hole = smooth(4.5, 3.0, np.hypot(hx, hy))
    base = fbm(1.4)
    c = lerp(col(118, 88, 58), col(142, 110, 76), base)
    c = c * (1 - 0.85 * hole[..., None])
    save("pegboard", c, base * 0.2 - hole * 2.0, 4.0)


def cast_metal():
    """1 m tile: rough cast iron / aluminium with oil grime (engine block)."""
    rough = fbm(0.7)
    grime = smooth(0.45, 0.85, fbm(2.2))
    c = lerp(col(92, 94, 96), col(122, 124, 126), rough)
    c = lerp(c, col(40, 36, 30), grime * 0.7)
    save("cast_metal", c, rough * 0.6 + grime * 0.2, 5.0)


def shop_sign():
    """2048x512 hand-painted sign on rusty sheet: TAMİR ATÖLYESİ."""
    from PIL import ImageDraw, ImageFont, ImageFilter
    W, H = 2048, 512
    global N
    keep = N
    N = 512
    rust = fbm(2.2)
    N = keep
    rust = np.array(Image.fromarray((rust * 255).astype(np.uint8)).resize((W, H))) / 255.0
    plate = lerp(col(180, 172, 150), col(205, 198, 176), rust)
    rust_mask = smooth(0.62, 0.8, rust)
    plate = lerp(plate, col(110, 60, 34), rust_mask)
    img = Image.fromarray((np.clip(plate, 0, 1) * 255).astype(np.uint8))
    d = ImageDraw.Draw(img)
    font = ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf", 175)
    text = "TAMİR ATÖLYESİ"
    bbox = d.textbbox((0, 0), text, font=font)
    tx = (W - (bbox[2] - bbox[0])) // 2
    ty = (H - (bbox[3] - bbox[1])) // 2 - bbox[1]
    layer = Image.new("L", (W, H), 0)
    ImageDraw.Draw(layer).text((tx, ty), text, font=font, fill=255)
    layer = layer.filter(ImageFilter.GaussianBlur(1.2))
    mask = np.array(layer) / 255.0 * (1 - 0.75 * rust_mask)
    mask = mask * (0.75 + 0.25 * (rust > 0.35))
    arr = np.array(img) / 255.0
    arr = lerp(arr, col(150, 30, 24), mask)
    border = np.zeros((H, W))
    border[:22, :] = border[-22:, :] = 1
    border[:, :22] = border[:, -22:] = 1
    arr = lerp(arr, col(40, 34, 30), border * 0.8)
    Image.fromarray((np.clip(arr, 0, 1) * 255).astype(np.uint8)).save(f"{OUT}/shop_sign_albedo.jpg", quality=90)
    print("saved shop_sign")


def storm_clouds():
    """1024 RGBA tileable cloud layer: dense dark storm clouds with thinner gaps (alpha)."""
    big = fbm(3.4)
    mid = fbm(2.8)
    density = np.clip(0.65 * big + 0.35 * mid, 0, 1)
    density = (density - density.min()) / (density.max() - density.min())
    alpha = 0.55 + 0.45 * smooth(0.2, 0.6, density)          # overcast: never fully clear
    shade = 0.5 + 0.5 * (1 - smooth(0.3, 0.95, density))      # thick parts darker
    rgb = np.stack([shade * 0.96, shade * 0.98, shade], -1)
    img = np.concatenate([np.clip(rgb, 0, 1), alpha[..., None]], -1)
    Image.fromarray((img * 255).astype(np.uint8), "RGBA").save(f"{OUT}/storm_clouds.png", optimize=True)
    print("saved storm_clouds")


def rain_streak():
    """16x128 soft vertical streak for stretched rain particles."""
    y, x = np.mgrid[0:128, 0:16]
    across = np.exp(-((x - 7.5) / 2.2) ** 2)
    along = np.sin(np.clip(y / 127.0, 0, 1) * np.pi) ** 0.6
    a = across * along
    img = np.stack([np.full_like(a, 0.85), np.full_like(a, 0.88), np.ones_like(a), a], -1)
    Image.fromarray((img * 255).astype(np.uint8), "RGBA").save(f"{OUT}/rain_streak.png")
    print("saved rain_streak")


def smoke_puff():
    """128x128 soft smoke puff (RGBA) for particles."""
    noise = fbm(1.8, n=128)
    y, x = np.mgrid[0:128, 0:128]
    r = np.hypot(x - 63.5, y - 63.5) / 64.0
    a = np.clip(1 - r, 0, 1) ** 1.6 * (0.6 + 0.4 * noise)
    img = np.stack([np.full_like(a, 0.8), np.full_like(a, 0.8), np.full_like(a, 0.8), np.clip(a, 0, 1)], -1)
    Image.fromarray((img * 255).astype(np.uint8), "RGBA").save(f"{OUT}/smoke_puff.png")
    print("saved smoke_puff")


def ruin_facade():
    """Tile = 8 m wide x 6 m tall facade: 4 window columns x 2 floors, dark/broken windows,
    floor slab bands, rain streaks. Used with tiling (1/8, 1/6) on distant ruins."""
    y, x = np.mgrid[0:N, 0:N]
    colw, rowh = N // 4, N // 2
    cx, cy = x % colw, y % rowh
    col, row = x // colw, y // rowh
    win = (np.abs(cx - colw / 2) < colw * 0.25) & (cy > rowh * 0.22) & (cy < rowh * 0.7)
    cell = (col * 3 + row * 5) % 7
    boarded = win & (cell == 2)
    missing_wall = smooth(0.72, 0.8, fbm(2.6, highpass=1.0))       # blown-out patches
    base = fbm(2.0)
    streak = fbm(1.6, aniso=(0.06, 1.0))
    c = lerp(col_rgb := np.array([0.36, 0.36, 0.35]), np.array([0.5, 0.49, 0.47]), base)
    c = c * (1 - 0.25 * smooth(0.45, 0.85, streak)[..., None])
    slab = smooth(10, 0, np.minimum(cy, rowh - 1 - cy))
    c = c * (1 - 0.25 * slab[..., None])
    c = np.where(win[..., None], np.array([0.03, 0.03, 0.035]), c)
    c = np.where(boarded[..., None], np.array([0.22, 0.17, 0.12]), c)
    c = lerp(c, np.array([0.05, 0.05, 0.055]), missing_wall * 0.85)
    h = base * 0.3 - win * 2.0 - missing_wall * 1.5 + slab * 0.4
    save("ruin_facade", c, h, 3.0)


if __name__ == "__main__":
    ground_dirt()
    concrete_slab()
    concrete_wall()
    corrugated_rust()
    rusty_steel()
    concrete_block()
    wood_planks()
    rubber_tire()
    chipped_paint()
    pegboard()
    cast_metal()
    shop_sign()
    storm_clouds()
    rain_streak()
    smoke_puff()
    ruin_facade()
