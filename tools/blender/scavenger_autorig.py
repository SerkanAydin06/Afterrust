import bpy, sys, math
from mathutils import Vector, Matrix
src, out = sys.argv[-2], sys.argv[-1]
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=src)
mesh=[o for o in bpy.context.scene.objects if o.type=='MESH'][0]
mesh.name="Scavenger"
# bake transform, feet to z=0
bpy.context.view_layer.objects.active=mesh; mesh.select_set(True)
bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
zmin=min(v.co.z for v in mesh.data.vertices)
for v in mesh.data.vertices: v.co.z-=zmin
bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.mesh.remove_doubles(threshold=0.0005); bpy.ops.object.mode_set(mode='OBJECT')
V=[v.co.copy() for v in mesh.data.vertices]
H=max(v.z for v in V)
def cen(pred):
    pts=[v for v in V if pred(v)]
    s=Vector((0,0,0))
    for p in pts: s+=p
    return s/len(pts), len(pts)
def z(f): return H*f
# --- legs (side +1 = left = +X)
def leg(sd, f):
    c,_=cen(lambda v: v.x*sd>0.02 and abs(v.z-z(f))<H*0.01); return c
# --- arm line fit from arm-only vertices
def arm(sd):
    pts=[v for v in V if v.x*sd>0.27 and z(0.46)<v.z<z(0.69)]
    c=Vector((0,0,0))
    for p in pts: c+=p
    c/=len(pts)
    # principal axis via power iteration
    d=Vector((sd*0.6,0,-0.8)).normalized()
    for _ in range(30):
        n=Vector((0,0,0))
        for p in pts:
            q=p-c; n+=q*q.dot(d)
        d=n.normalized()
    if d.z>0: d=-d
    tmin=min((p-c).dot(d) for p in pts); tmax=max((p-c).dot(d) for p in pts)
    tip=c+d*tmax
    # extend up to shoulder height
    sh_z=z(0.775)
    t_sh=(sh_z-c.z)/d.z
    shoulder=c+d*t_sh
    shoulder.y=min(shoulder.y,-0.06)
    return shoulder, tip
J={}
J['Hips']=Vector((0,-0.07,z(0.50)))
J['Spine']=Vector((0,J['Hips'].y,z(0.56)))
J['Spine1']=Vector((0,J['Hips'].y,z(0.63)))
J['Spine2']=Vector((0,-0.07,z(0.70)))
J['Neck']=Vector((0,-0.06,z(0.835)))
J['Head']=Vector((0,-0.07,z(0.875)))
J['HeadTop_End']=Vector((0,-0.08,H))
for sd,S in ((1,'Left'),(-1,'Right')):
    hip=Vector((sd*0.10,-0.06,z(0.475)))
    ank=leg(sd,0.075); ank.z=z(0.075)
    ank_c,_=cen(lambda v: v.x*sd>0.02 and abs(v.z-z(0.09))<H*0.01 and v.y>-0.13)
    ank=Vector((ank_c.x,ank_c.y,z(0.075)))
    knee=leg(sd,0.27); knee.z=z(0.27)
    toe=Vector((ank.x, -0.17, z(0.025))); toe_end=Vector((ank.x,-0.28,z(0.025)))
    sh,tip=arm(sd)
    v=tip-sh
    J[S+'UpLeg']=hip; J[S+'Leg']=knee; J[S+'Foot']=ank; J[S+'ToeBase']=toe; J[S+'Toe_End']=toe_end
    J[S+'Shoulder']=Vector((sd*0.05,-0.06,z(0.775)))
    J[S+'Arm']=sh; J[S+'ForeArm']=sh+v*0.44; J[S+'Hand']=sh+v*0.80; J[S+'HandEnd']=tip
for k,p in J.items(): print(f"{k:14s} {p.x:+.3f} {p.y:+.3f} {p.z:+.3f}")
# --- armature
bpy.ops.object.armature_add(enter_editmode=True, location=(0,0,0))
arm_obj=bpy.context.object; arm_obj.name="Armature"; amt=arm_obj.data; amt.name="Armature"
eb=amt.edit_bones; eb.remove(eb[0])
chain=[('Hips',None,'Spine'),('Spine','Hips','Spine1'),('Spine1','Spine','Spine2'),('Spine2','Spine1','Neck'),
       ('Neck','Spine2','Head'),('Head','Neck','HeadTop_End')]
for S in ('Left','Right'):
    chain+=[(S+'UpLeg','Hips',S+'Leg'),(S+'Leg',S+'UpLeg',S+'Foot'),(S+'Foot',S+'Leg',S+'ToeBase'),(S+'ToeBase',S+'Foot',S+'Toe_End'),
            (S+'Shoulder','Spine2',S+'Arm'),(S+'Arm',S+'Shoulder',S+'ForeArm'),(S+'ForeArm',S+'Arm',S+'Hand'),(S+'Hand',S+'ForeArm',S+'HandEnd')]
for name,parent,tail in chain:
    b=eb.new(name); b.head=J[name]; b.tail=J[tail]
    if parent: b.parent=eb[parent]; b.use_connect=False
for name,parent,tail in chain:
    b=eb[name]
    # consistent roll: lateral axis = X for limbs/spine
    b.align_roll(Vector((0,-1,0)) if abs(b.vector.normalized().z)>0.5 else Vector((0,0,1)))
bpy.ops.object.mode_set(mode='OBJECT')
# --- skin with automatic (heat) weights
bpy.ops.object.select_all(action='DESELECT')
mesh.select_set(True); arm_obj.select_set(True); bpy.context.view_layer.objects.active=arm_obj
bpy.ops.object.parent_set(type='ARMATURE_AUTO')
unweighted=sum(1 for v in mesh.data.vertices if not any(g.weight>0.01 for g in v.groups))
print("vertices",len(V),"unweighted",unweighted)
bpy.ops.wm.save_as_mainfile(filepath=out)
