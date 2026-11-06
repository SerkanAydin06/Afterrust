import random, math, json
r=random.Random(2049)
B=[]   # (x, z, yaw, w, d, h, topW, topD, topH, topOffX, topOffZ, cutW, cutH)
a=0.0
while a<360:
    ang=math.radians(a)
    rad=r.uniform(48,88)
    x=math.cos(ang)*rad; z=math.sin(ang)*rad+6   # yard centre is around z=6
    w=r.choice([8,10,12,14,16]); d=r.choice([8,10,12]); h=r.choice([10,12,14,18,22,26,30])
    yaw=round(-a+90+r.uniform(-12,12),1)
    tw=round(w*r.uniform(0.35,0.7)); td=round(d*r.uniform(0.4,0.8)); th=r.choice([2,4,6])
    ox=round(r.uniform(-(w-tw)/2,(w-tw)/2),1); oz=round(r.uniform(-(d-td)/2,(d-td)/2),1)
    cw=round(w*r.uniform(0.25,0.45)); ch=round(h*r.uniform(0.3,0.6))   # collapsed corner: part of the block lower
    B.append((round(x,1),round(z,1),yaw,w,d,h,tw,td,th,ox,oz,cw,ch))
    a+=r.uniform(13,22)
json.dump(B,open('ruins.json','w'))
print(len(B))
cs=",\n            ".join("new Ruin({},{},{},{},{},{},{},{},{},{},{},{},{})".format(*[str(v)+"f" for v in b]) for b in B)
open('ruins_cs.txt','w').write(cs)
