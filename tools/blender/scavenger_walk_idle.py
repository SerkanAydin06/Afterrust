import bpy, sys, math
blend, fbx = sys.argv[-2], sys.argv[-1]
bpy.ops.wm.open_mainfile(filepath=blend)
sc=bpy.context.scene; sc.render.fps=30
arm=bpy.data.objects['Armature']; pb=arm.pose.bones
for b in pb: b.rotation_mode='XYZ'
def upd(): bpy.context.view_layer.update()
def wp(n): upd(); return arm.matrix_world@pb[n].head
def reset():
    for b in pb: b.rotation_euler=(0,0,0); b.location=(0,0,0)
# axis sign to bring arms down toward body
h0=wp('LeftHand'); pb['LeftArm'].rotation_euler=(0,0,math.radians(20)); h1=wp('LeftHand'); reset()
AZ= 1 if abs(h1.x)<abs(h0.x) else -1
h0=wp('RightHand'); pb['RightArm'].rotation_euler=(0,0,math.radians(20)); h1=wp('RightHand'); reset()
AZR= 1 if abs(h1.x)<abs(h0.x) else -1
print("arm down signs",AZ,AZR)
F=1  # +X swings forward (measured)
def R(n,x=0,y=0,z=0):
    pb[n].rotation_euler=(math.radians(x),math.radians(y),math.radians(z))
def key(fr):
    for b in pb:
        b.keyframe_insert('rotation_euler',frame=fr); b.keyframe_insert('location',frame=fr)
def arms_down(lx, rx, fl, fr_):
    R('LeftArm', F*lx, 0, AZ*28); R('RightArm', F*rx, 0, AZR*28)
    R('LeftForeArm', F*fl); R('RightForeArm', F*fr_)
def new_action(name):
    arm.animation_data_create()
    act=bpy.data.actions.new(name); arm.animation_data.action=act; return act
acts=[]
# ---- Walk: 32 frames loop
N=32
act=new_action("Walk"); acts.append(act)
for f in range(N+1):
    p=f/N; reset(); c=math.cos(2*math.pi*p)
    def leg(side, ph):
        th=24*math.cos(2*math.pi*ph)
        sw=max(0.0, math.sin(2*math.pi*(ph-0.5)))   # swing phase 0.5..1
        ld=max(0.0, math.sin(4*math.pi*ph)) if ph<0.25 else 0.0  # loading response
        knee=6+55*sw**1.3+10*ld
        foot=-0.35*th+0.3*knee - 6
        R(side+'UpLeg', F*th); R(side+'Leg', -F*knee); R(side+'Foot', F*foot)
    leg('Left',p); leg('Right',(p+0.5)%1)
    pb['Hips'].location=(0, -0.018*math.cos(4*math.pi*p), 0)
    R('Hips', 0, 6*c, 2.5*math.sin(2*math.pi*p))
    R('Spine', 2, -3*c, 0); R('Spine1', 0, -3*c, 0); R('Neck', -2, 2*c, 0)
    arms_down(-16*c, 16*c, 18+8*max(0,-c), 18+8*max(0,c))
    key(f+1)
# ---- Idle: 60 frames loop
N=60
act=new_action("Idle"); acts.append(act)
for f in range(N+1):
    p=f/N; s=math.sin(2*math.pi*p); reset()
    R('Spine1', 1.5*s); R('Spine2', 1.0*s); R('Neck', -1.0*s, 2*math.sin(2*math.pi*p+1))
    arms_down(2, 2, 15+2*s, 15+2*s)
    R('LeftUpLeg', 0,0,0); key(f+1)
for a in acts:
    for fc in (a.fcurves if hasattr(a,'fcurves') else []):
        for k in fc.keyframe_points: k.interpolation='LINEAR'
# stash actions to NLA so the exporter sees them all
for a in acts:
    t=arm.animation_data.nla_tracks.new(); t.strips.new(a.name, 1, a); a.use_fake_user=True
arm.animation_data.action=None
reset(); upd()
bpy.ops.object.select_all(action='SELECT')
bpy.ops.export_scene.fbx(filepath=fbx, use_selection=True, apply_scale_options='FBX_SCALE_ALL',
    add_leaf_bones=False, bake_anim=True, bake_anim_use_all_actions=True, bake_anim_use_nla_strips=False,
    bake_anim_force_startend_keying=True, bake_anim_simplify_factor=0.0, path_mode='STRIP',
    object_types={'ARMATURE','MESH'}, mesh_smooth_type='FACE')
bpy.ops.wm.save_as_mainfile(filepath=blend.replace('.blend','_anim.blend'))
