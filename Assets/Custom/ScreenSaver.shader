Shader "Unlit/ScreenSaver"
{
	Properties
	{
		_Noise("Texture", 2D) = "white" {}
		_Mat("Texture", 2D) = "white" {}
		_Buildings("Texture", 2D) = "white" {}
		_Wood("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _Noise;
			sampler2D _Mat;
			sampler2D _Buildings;
			sampler2D _Wood;

			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			// Venice. Created by Reinder Nijhoff 2013
			// @reindernijhoff
			//
			// https://www.shadertoy.com/view/MdXGW2
			//
			// My attempt to create a procedural city with a lot of lights. The city is inspired by Venice. 
			// The shader is a combination of my shaders: https://www.shadertoy.com/view/Mdf3zM and 
			// https://www.shadertoy.com/view/lslGDB.
			// (I have never been in Venice btw)
			//

			// #define SHOW_ORNAMENTS
			#define SHOW_GALLERY
			#define SHOW_LIGHTS
			#define SHOW_BRIDGES
			#define SHOW_MOON_AND_CLOUDS

						//----------------------------------------------------------------------

			#define BUMPFACTOR 0.2
			#define EPSILON 0.1
			#define BUMPDISTANCE 200.

			#define CAMERASPEED 15.

			#define BUILDINGSPACING 20.
			#define MAXBUILDINGINSET 12.

			#define GALLERYHEIGHT 10.5
			#define GALLERYINSET 2.5

			float time;

			float hash(float n) {
				return frac(sin(n)*32.5454412211233);
			}
			float2 hash2(float n) {
				return frac(sin(float2(n, n + 1.0))*float2(11.1451239123, 34.349430423));
			}
			float3 hash3(float n) {
				return frac(sin(float3(n, n + 1.0, n + 2.0))*float3(84.54531253, 42.145259123, 23.349041223));
			}

			float noise(in float2 x) {
				float2 p = floor(x);
				float2 f = frac(x);

				float2 uv = p.xy + f.xy*f.xy*(3.0 - 2.0*f.xy);

				return -1.0 + 2.0*tex2D(_Noise, (uv + 0.5) / 256.0).x;
			}

			float noise(in float3 x)
			{
				float  z = x.z*64.0;
				float2 offz = float2(0.317, 0.123);
				float2 uv1 = x.xy + offz * floor(z);
				float2 uv2 = uv1 + offz;
				return lerp(tex2D(_Noise, uv1).x, tex2D(_Noise, uv2).x, frac(z)) - 0.5;
			}

			const float2x2 m2 = float2x2(0.80, -0.60, 0.60, 0.80);

			const float3x3 m3 = float3x3(0.00, 0.80, 0.60,
				-0.80, 0.36, -0.48,
				-0.60, -0.48, 0.64);

			float fbm(float3 p) {
				float f = 0.0;
				f += 0.5000*noise(p); p = mul(m3, p)*2.02;
				f += 0.2500*noise(p); p = mul(m3, p)*2.03;
				f += 0.1250*noise(p); p = mul(m3, p)*2.01;
				f += 0.0625*noise(p);
				return f / 0.9375;
			}

			//----------------------------------------------------------------------
			// distance functions

			float sdBox(float3 p, float3 b) {
				float3 d = abs(p) - b;
				return min(max(d.x, max(d.y, d.z)), 0.0) +
					length(max(d, 0.0));
			}
			float sdSphere(float3 p, float s) {
				return length(p) - s;
			}
			float udBox(float3 p, float3 b) {
				return length(max(abs(p) - b, 0.0));
			}
			float sdCylinderXY(float3 p, float2 h) {
				return length(p.xy) - h.x; //max( length(p.xy)-h.x, abs(p.z)-h.y );
			}
			float sdCylinderXZ(float3 p, float2 h) {
				return max(length(p.xz) - h.x, abs(p.y) - h.y);
			}
			float sdTriPrism(float3 p, float2 h) {
				float3 q = abs(p);
				return max(q.z - h.y, max(q.x*0.866025 + p.y*0.5, -p.y) - h.x*0.5);
			}

			//----------------------------------------------------------------------

			float opS(float d1, float d2) {
				return max(-d2, d1);
			}
			float opU(float d1, float d2) {
				return min(d2, d1);
			}
			float2 opU(float2 d1, float2 d2) {
				return (d1.x<d2.x) ? d1 : d2;
			}
			float opI(float d1, float d2) {
				return max(d1, d2);
			}

			//----------------------------------------------------------------------
			// building functions

			float getXoffset(float z) {
				return 20.*sin(z*0.02);
			}

			float2 getBuildingInfo(in float3 pos) {
				float2 res;
				// base index	
				res.x = floor(pos.z / BUILDINGSPACING + 0.5);
				// base z coord
				res.y = res.x * BUILDINGSPACING;

				// negative index for buildings at the right side
				res.x *= sign(pos.x + getXoffset(pos.z));

				return res;
			}

			float4 getBuildingParams(in float buildingindex) {
				float3 h = hash3(buildingindex);
				return float4(
					20. + 4.5*floor(h.x*7.),	 // height
					h.y*MAXBUILDINGINSET,
					step(h.z, 0.5),				 // sidewalk
					step(abs(h.z - 0.4), 0.25)		 // balcony
				);
			}

			float baseBuilding(in float3 pos, in float h) {
				float3 tpos = float3(pos.z, pos.y, pos.x);

				float res =
					opS(
						// main building
						udBox(tpos, float3(8.75, h, 8.75)),
						// windows
						opS(
							opU(
								sdBox(float3(fmod(tpos.x + 1.75, 3.5) - 1.75, fmod(tpos.y + 4.5, 9.) - 2.5, tpos.z - 5.), float3(1., 2., 4.)),
								sdCylinderXY(float3(fmod(tpos.x + 1.75, 3.5) - 1.75, fmod(tpos.y + 4.5, 9.) - 4.5, tpos.z - 5.), float2(1., 4.))
							),
							udBox(tpos + float3(0., -h, 0.), float3(9.0, 1.0, 9.0))
						)
					);

				res =
					opU(
						res,
						opI( // main building windows
							udBox(tpos, float3(8.75, h, 8.75)),
							opU(
								udBox(float3(fmod(tpos.x + 1.75, 3.5) - 1.75, tpos.y, tpos.z - 8.45), float3(0.05, h, 0.05)),
								udBox(float3(tpos.x, fmod(tpos.y + 0.425, 1.75) - 0.875, tpos.z - 8.45), float3(10.0, 0.05, 0.05))
							)
						)
					);
				return res;
			}

			float baseGallery(in float3 pos) {
				float3 tpos = float3(pos.z, pos.y, pos.x);

				float res =
					opU(
						opS(
							udBox(tpos + float3(0., 0., -GALLERYINSET), float3(8.75, GALLERYHEIGHT, 0.125)),
							opU(
								sdBox(float3(fmod(tpos.x + 1.75, 3.5) - 1.75, tpos.y - 5., tpos.z - 5.), float3(1.6, 3., 10.)),
								sdCylinderXY(float3(fmod(tpos.x + 1.75, 3.5) - 1.75, tpos.y - 8., tpos.z - 5.), float2(1.6, 10.))
							)
						),
						sdTriPrism(float3(tpos.z + 3.4, -44.4 + 3.9*tpos.y, tpos.x), float2(7.5, 8.7))
					);

				return res;
			}

			float baseBalcony(in float3 pos, in float h) {
				float res = opI(
					// main building
					udBox(pos, float3(9.0, h, 9.0)),
					// balcony
					sdBox(float3(pos.x, fmod(pos.y + 4.5, 9.) - 7.5, pos.z - 5.), float3(40., 0.5, 40.))
				);
				return res;
			}

			float baseBridge(in float3 pos) {
				pos.x *= 0.38;
				float res =
					opS(
						opU(
							sdBox(pos, float3(4., 2., 2.5)),
							sdTriPrism(float3(pos.x, -8. + 3.*pos.y, pos.z), float2(4.5, 2.5))
						),
						sdCylinderXY(pos + float3(0., 1.5, 0.), float2(3.8, 3.))
					);
				return res;
			}

			// dinstancefield definitions

			float mapSimpleTerrain(in float3 p) {
				p.x += getXoffset(p.z);
				p.x = -abs(p.x);
				float2 res = float2(udBox(float3(p.x + 30., p.y - 1., p.z), float3(20., 100.25, 99999.)), 1.);

#ifdef SHOW_BRIDGES
				float zcenter = fmod(p.z + 60., 120.) - 70.;
				res = opU(res, float2(baseBridge(float3(p.x, p.y, zcenter)), 8.)); // bridge
#endif

				return min(res.x, p.y + 10.);
			}

			float2 mapTerrain(in float3 p) {
				float2 buildingInfo = getBuildingInfo(p);
				float4 buildingParams = getBuildingParams(buildingInfo.x);

				float3 pos = p;
				pos.x += getXoffset(pos.z);
				pos.x = -abs(pos.x);

				float2 res = float2(udBox(float3(pos.x + 30., pos.y, pos.z), float3(20., 0.25, 99999.)), 1.); // ground

				float z = buildingInfo.y;
				float zcenter = fmod(pos.z + 10., 20.) - 10.;

#ifdef SHOW_BRIDGES
				res = opU(res, float2(baseBridge(float3(pos.x, pos.y, fmod(pos.z + 60., 120.) - 70.)), 8.)); // bridge
#endif

				res = opU(res, float2(sdSphere(float3(pos.x + 11.5, pos.y - 6.0, zcenter), 0.5), 3.)); // light	
				res = opU(res, float2(sdSphere(float3(pos.x + 11.5, pos.y - 5.4, zcenter + 0.6), 0.35), 3.)); // light	
				res = opU(res, float2(sdSphere(float3(pos.x + 11.5, pos.y - 5.4, zcenter - 0.6), 0.35), 3.)); // light

				res = opU(res, float2(sdCylinderXZ(float3(pos.x + 11.5, pos.y, zcenter), float2(0.1, 6.0)), 4.)); // 

				pos += float3(28.75 + buildingParams.y, 2.5, 0.);
				res = opU(res, float2(baseBuilding(float3(pos.x, pos.y, zcenter), buildingParams.x + 2.5), 2.));

#ifdef SHOW_ORNAMENTS
				res = lerp(res, opU(res, float2(baseBalcony(float3(pos.x, pos.y, zcenter), buildingParams.x + 2.5), 9.)), buildingParams.w);
#endif

#ifdef SHOW_GALLERY
				pos.x += -8.75 - GALLERYINSET;
				res = lerp(res, opU(res, float2(baseGallery(float3(pos.x, pos.y, zcenter)), 5.)), buildingParams.z);
#endif	

				return float2(min(res.x, 11. - zcenter), res.y);
			}

			float waterHeightMap(float2 pos) {
				float2 posm = 0.02*mul(pos, m2);
				posm.x += 0.001*time;
				float f = fbm(float3(posm*1.9, time*0.01));
				float height = 0.5 + 0.1*f;
				height += 0.05*sin(posm.x*6.0 + 10.0*f);

				return  height;
			}

			// intersection functions			

			bool intersectPlane(float3 ro, float3 rd, float height, out float dist) {
				if (rd.y == 0.0) {
					return false;
				}

				float d = -(ro.y - height) / rd.y;
				d = min(100000.0, d);
				if (d > 0.) {
					dist = d;
					return true;
				}
				return false;
			}

			bool intersectSphere(in float3 ro, in float3 rd, in float4 sph, out float3 normal) {
				float3  ds = ro - sph.xyz;
				float bs = dot(rd, ds);
				float cs = dot(ds, ds) - sph.w*sph.w;
				float ts = bs * bs - cs;

				if (ts > 0.0) {
					ts = -bs - sqrt(ts);
					if (ts>0.) {
						normal = normalize(((ro + ts * rd) - sph.xyz) / sph.w);
						return true;
					}
				}

				return false;
			}

			float3 intersect(const float3 ro, const float3 rd) {
				float maxd = 1500.0;
				float precis = 0.01;
				float h = precis * 2.0;
				float t = 0.0;
				float d = 0.0;
				float m = 1.0;
				for (int i = 0; i<140; i++) {
					if (abs(h)<precis || t>maxd) break; {
						t += h;
						float2 mt = mapTerrain(ro + rd * t);
						h = 0.96*mt.x;
						m = mt.y;
					}
				}

				if (t>maxd) m = -1.0;
				return float3(t, d, m);
			}

			float intersectSimple(const float3 ro, const float3 rd) {
				float maxd = 10000.0;
				float precis = 0.01;
				float h = precis * 2.0;
				float t = 0.0;
				for (int i = 0; i<50; i++) {
					if (abs(h)<precis || t>maxd) break; {
						t += h;
						h = mapSimpleTerrain(ro + rd * t);
					}
				}

				return t;
			}

			float3 calcNormal(const float3 pos) {
				float3 eps = float3(0.1, 0.0, 0.0);

				return normalize(float3(
					mapTerrain(pos + eps.xyy).x - mapTerrain(pos - eps.xyy).x,
					mapTerrain(pos + eps.yxy).x - mapTerrain(pos - eps.yxy).x,
					mapTerrain(pos + eps.yyx).x - mapTerrain(pos - eps.yyx).x));
			}

			float calcAO(const float3 pos, const float3 nor) {
				float totao = 0.0;
				float sca = 1.0;
				for (int aoi = 0; aoi<5; aoi++) {
					float hr = 0.01 + 0.05*float(aoi);
					float3 aopos = nor * hr + pos;
					float dd = mapTerrain(aopos).x;
					totao += -(dd - hr)*sca;
					sca *= 0.75;
				}
				return clamp(1.0 - 4.0*totao, 0.0, 1.0);
			}

			float4 texcube(sampler2D sam, in float3 p, in float3 n)
			{
				float4 x = tex2D(sam, p.yz);
				float4 y = tex2D(sam, p.zx);
				float4 z = tex2D(sam, p.xy);

				return x * abs(n.x) + y * abs(n.y) + z * abs(n.z);
			}

			void getSkyColor(in float3 rd, out float3 bgcol, out float3 col) {
				float3 lig = normalize(float3(-2.5, 1.7, 2.5));

				bgcol = 1.1*float3(0.15, 0.15, 0.4) - rd.y*0.4;
				bgcol *= 0.3;
				float moon = clamp(dot(rd, lig), 0.0, 1.0);
				bgcol += float3(2.0, 1.5, 0.8)*0.015*pow(moon, 32.0);

				col = bgcol;

#ifdef SHOW_MOON_AND_CLOUDS	
				// moon!
				float3 normal;
				if (intersectSphere(float3(0., 0., 0.), rd, float4(lig, 0.03), normal)) {
					float l = dot(normalize(float3(2.2, -1.9, 0.5)), normal)*(0.4 + tex2D(_Buildings, normal.xy*0.5).y);
					col += 0.2*clamp(2.5*float3(2.0, 1.5, 0.8)*clamp(l, 0.0, 1.), float3(0,0,0), float3(1,1,1));
				}

				// cloud function by inigo: https://www.shadertoy.com/view/Mds3z2 
				float2 cuv = rd.xz*(100.0) / rd.y;
				float cc = tex2D(_Buildings, 0.0001*cuv + 0.1 + 0.0013*time).x;
				cc = 0.65*cc + 0.35*tex2D(_Buildings, 0.0001*2.0*cuv + 0.0013*.5*time).x;
				cc = smoothstep(0.3, 1.0, 1.1*cc);
				col = lerp(col, 0.1*float3(0.05, 0.05, 0.4), 0.99*cc);
#endif
			}

			//-----------------------------------------------------

			float3 path(float _time) {
				float z = _time * CAMERASPEED;
				return float3(-getXoffset(z) + 5.*cos(_time*0.1), 1.25, z);
			}

			fixed4 frag(v2f i) : SV_Target{

				float2 fragCoord = i.uv;

				time = _Time.y + 43.;
				float2 q = fragCoord.xy /_ScreenParams.xy;
				float2 p = -1.0 + 2.0*q;
				p.x *= _ScreenParams.x / _ScreenParams.y;

				float3 iMouse = float3(0, 0, 0);
				// camera	
				float off = step(0.001, iMouse.z)*6.0*iMouse.x / _ScreenParams.x;
				time += off;
				float3 ro = path(time + 0.0);
				float3 ta = path(time + 1.6);

				ta.y *= 1.1 + 0.25*sin(0.09*time);
				float roll = 0.3*sin(1.0 + 0.07*time);

				// camera tx
				float3 cw = normalize(ta - ro);
				float3 cp = float3(sin(roll), cos(roll), 0.0);
				float3 cu = normalize(cross(cw, cp));
				float3 cv = normalize(cross(cu, cw));

				float3 rd = normalize(p.x*cu + p.y*cv + 2.1*cw);


				//-----------------------------------------------------
				// render
				//-----------------------------------------------------

				// raymarch
				float distSimple = intersectSimple(ro, rd);
				bool reflection = false;

				float dist, totaldist = 0., depth = 0.;
				float3 normal, tmat, lp, lig;

				if (intersectPlane(ro, rd, 0., dist) && dist < distSimple) {
					ro = ro + rd * dist;
					totaldist = dist;

					depth = mapTerrain(ro).x;

					float2 coord = ro.xz;
					float2 dx = float2(EPSILON, 0.);
					float2 dz = float2(0., EPSILON);

					float bumpfactor = BUMPFACTOR * (1. - smoothstep(0., BUMPDISTANCE, dist));

					normal = float3(0., 1., 0.);
					normal.x = -bumpfactor * (waterHeightMap(coord + dx) - waterHeightMap(coord - dx)) / (2. * EPSILON);
					normal.z = -bumpfactor * (waterHeightMap(coord + dz) - waterHeightMap(coord - dz)) / (2. * EPSILON);
					normal = normalize(normal);

					rd = reflect(rd, normal);
					reflection = true;
				}

				// intersect scene	
				tmat = intersect(ro, rd);
				totaldist += tmat.x;

				// sky	 
				float3 col, bgcol;
				getSkyColor(rd, bgcol, col);

				float3 pos = ro + tmat.x*rd;

				if (tmat.z>-0.5 && totaldist < 500.) {
					// info building hit
					float2 buildingInfo = getBuildingInfo(pos);
					float4 buildingParams = getBuildingParams(buildingInfo.x);

					float z = buildingInfo.y;
					lp = float3(11.5*sign(buildingInfo.x) - getXoffset(z), 6.0, z);
					lig = normalize(lp - pos);

					// geometry
					float3 nor = calcNormal(pos);

					// material
					float3 mate, origmate;
					float3 matpos = pos * 0.3;

#ifdef SHOW_GALLERY
					if (tmat.z == 5.)
						mate.xyz = texcube(_Wood, matpos, nor).xyz*0.2;
					else
#endif
						origmate = mate.xyz = texcube(_Mat, matpos, nor).xyz*0.4;

					bool aboveGallery = false;

					if (tmat.z == 3.) mate.xyz = 160.*float3(1.30, 1.10, 0.40);
					else if (tmat.z == 2.) mate.xyz *=
						clamp(4.*tex2D(_Buildings, buildingInfo.x*float2(1.4231153121, 1.4231153121)).xyz
							, float3(0,0,0), float3(1.,1,1));

					// lighting
					float occ = calcAO(pos, nor);
					float amb = clamp(0.5 + 0.5*nor.y, 0.0, 1.0);
					float dif = max(dot(nor, lig), 0.0);
					if (tmat.z == 5. && pos.y > GALLERYHEIGHT - 2.6) {
						dif = abs(dot(nor, lig));
						mate.xyz = float3(0.3, 0., 0.);
					}
					dif /= dot(lp - pos, lp - pos);

					float bac = max(0.2 + 0.8*dot(nor, normalize(float3(-lig.x, 0.0, -lig.z))), 0.0);

					if (buildingParams.z == 1. && pos.y > GALLERYHEIGHT) {
						aboveGallery = true;
					}
					float3 lcol = aboveGallery ? float3(2.9, 1.65, 0.65) : float3(1.30, 0.60, 0.40);

					// lights
					float3 brdf = float3(0.0, 0, 0);
					brdf += (60.0*dif)*lcol;
					brdf += (0.1*amb)*float3(0.10, 0.15, 0.30);
					brdf += (0.1*bac)*float3(0.09, 0.03, 0.01);

					// surface-light interacion
					col = (mate.xyz*brdf)*occ;

					// in room ?
					float isLeft = sign(buildingInfo.x);

					if (((pos.x + getXoffset(pos.z))*isLeft > buildingParams.y + 20.25 &&
						abs(pos.z - buildingInfo.y) < 8.5 &&
						pos.y < buildingParams.x - 0.5) || false) {

						float2 roomcoord = pos.zy;
						roomcoord.x = floor((roomcoord.x - buildingInfo.y + 5.) / 3.5) * 3.5 +
							floor((buildingInfo.y + 5.) / 10.) * 10.;
						roomcoord.y = floor(roomcoord.y / 9.) * 9.;

						if (noise(float3(roomcoord*1.15321*isLeft, time*0.0005)) > -0.1) {
							float3 rlc = float3(
								(buildingParams.y + 3. + 20.25)*isLeft - getXoffset(roomcoord.x - 5.),
								roomcoord.y + 5.5,
								roomcoord.x - 5.);
							float3 ld = rlc - pos;
							dif = max(dot(nor, normalize(ld)), 0.0) / dot(ld, ld);
							col += origmate * (dif*120.)*tex2D(_Buildings, roomcoord*0.1231).xyz;
						}
					}

#ifdef SHOW_LIGHTS
					// and extra lights!
					float basez = floor((pos.z) / 2.)*2. - 2.0;
					for (int i = 0; i<3; i++) {
						buildingInfo = getBuildingInfo(float3(pos.x, pos.y, basez));
						// check if building lights here
						if (abs(basez - buildingInfo.y) > 8.75 ||
							noise(buildingInfo) > 0.15) {
							basez += 2.;
							continue;
						}
						buildingParams = getBuildingParams(buildingInfo.x);
						float3 rlc = float3((buildingParams.y - 1. + 20.25)*isLeft - getXoffset(basez),
							7.7 - 1.5*abs(sin(basez*0.3)), basez);
						float3 ld = rlc - pos;
						dif = max(dot(nor, normalize(ld)), 0.0) / dot(ld, ld);
						col += mate.xyz*(dif*6.0)*tex2D(_Buildings, float2(basez*time*0.0001, basez*time*0.0001)*0.1231).xyz;
						basez += 2.;
					}
#endif

					if (reflection) {
						col = lerp(bgcol, col, exp(-0.00000001*pow(totaldist - dist, 3.0)));
						col *= 0.9*float3(0.8, 0.9, 1.)*(0.5 + clamp(depth*2., 0.0, 0.5));
						if (dist != totaldist) totaldist = dist;
					}
					col = lerp(bgcol, col, exp(-0.00000001*pow(totaldist, 3.0)));
				}


				//-----------------------------------------------------
				// postprocessing
				//-----------------------------------------------------
				// gamma
				col = clamp(col, 0.0, 1.0);
				col = pow(clamp(col, 0.0, 1.0), float3(0.45, 0.45, 0.45));

				col *= float3(1.03, 1.02, 1.0);
				col *= 0.5 + 0.5*pow(16.0*q.x*q.y*(1.0 - q.x)*(1.0 - q.y), 0.1);

				float4 fragColor = float4(col, 1.0);

				return fragColor;
			}

			ENDCG
		}
	}
}
