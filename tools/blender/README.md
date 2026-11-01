# Scavenger auto-rig (Blender, headless)
Meshy auto-rig ağırlıkları uzuvları "lastik gibi" uzatıyordu; bu betikler aynı Meshy mesh'ini Humanoid uyumlu (Mixamo adlı) iskeletle yeniden rigler.

```bash
pip install bpy   # Blender 5.x Python modülü
python3 scavenger_autorig.py -- <meshy_model.fbx> rigged.blend
python3 scavenger_walk_idle.py -- rigged.blend Scavenger_Rigged.fbx
```
- `scavenger_autorig.py`: mesh dilimlerinden eklem noktalarını bulur, 22 kemikli iskelet kurar, otomatik (heat) ağırlık verir.
- `scavenger_walk_idle.py`: döngülü `Walk` (32 kare, 30 fps) ve `Idle` (60 kare) animasyonlarını ekleyip FBX çıkarır.
- Kontrol görselleri: `QA/Rig/`.
