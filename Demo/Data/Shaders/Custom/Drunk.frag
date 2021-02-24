#version 420

in vec2 fragUV;
out vec4 out_Color;

uniform sampler2D BlitTex;
uniform vec4 BlitRect;

uniform float Speed = 1.0;
uniform float Intensity = 1.0;
uniform float Time = 0.0;


// 	<www.shadertoy.com/view/XsX3zB>
//	by Nikita Miropolskiy

/* discontinuous pseudorandom uniformly distributed in [-0.5, +0.5]^3 */
vec3 random3(vec3 c) {
	float j = 4096.0*sin(dot(c,vec3(17.0, 59.4, 15.0)));
	vec3 r;
	r.z = fract(512.0*j);
	j *= .125;
	r.x = fract(512.0*j);
	j *= .125;
	r.y = fract(512.0*j);
	return r-0.5;
}

const float F3 =  0.3333333;
const float G3 =  0.1666667;
float snoise(vec3 p) {

	vec3 s = floor(p + dot(p, vec3(F3)));
	vec3 x = p - s + dot(s, vec3(G3));
	 
	vec3 e = step(vec3(0.0), x - x.yzx);
	vec3 i1 = e*(1.0 - e.zxy);
	vec3 i2 = 1.0 - e.zxy*(1.0 - e);
	 	
	vec3 x1 = x - i1 + G3;
	vec3 x2 = x - i2 + 2.0*G3;
	vec3 x3 = x - 1.0 + 3.0*G3;
	 
	vec4 w, d;
	 
	w.x = dot(x, x);
	w.y = dot(x1, x1);
	w.z = dot(x2, x2);
	w.w = dot(x3, x3);
	 
	w = max(0.6 - w, 0.0);
	 
	d.x = dot(random3(s), x);
	d.y = dot(random3(s + i1), x1);
	d.z = dot(random3(s + i2), x2);
	d.w = dot(random3(s + 1.0), x3);
	 
	w *= w;
	w *= w;
	d *= w;
	 
	return dot(d, vec4(52.0));
}

float snoiseFractal(vec3 m) {
	return   0.5333333* snoise(m)
				+0.2666667* snoise(2.0*m)
				+0.1333333* snoise(4.0*m)
				+0.0666667* snoise(8.0*m);
}

float calc(float x)
{
    return snoiseFractal(vec3(x, fragUV.y * 5.0 + Time * 60.0 * Speed, 0.0)) * Intensity;
}

float calc(float x, float y)
{
    return snoiseFractal(vec3(x, y, Time * 60.0 * Speed)) * Intensity;
}

void main()
{
    vec2 newUV = fragUV + vec2(0.0, calc(0.0)) / 100.0 * vec2(1.0, 1.778);
    vec2 blitUV1 = clamp(mix(BlitRect.xy, BlitRect.zw, fragUV), 0.0, 1.0);
    vec2 blitUV2 = mix(BlitRect.xy, BlitRect.zw, clamp(newUV - vec2(0.025 * Intensity / 4.0, 0.0), 0.0, 1.0));
    vec2 blitUV3 = mix(BlitRect.xy, BlitRect.zw, clamp(newUV - vec2(0.05 * Intensity / 4.0, 0.0), 0.0, 1.0));
    vec4 color1 = texture(BlitTex, blitUV1);
    vec4 color2 = texture(BlitTex, blitUV2);
    vec4 color3 = texture(BlitTex, blitUV3);
    color2.a *= 0.62;
    color3.a *= 0.38;
    color1.rgb *= color1.a;
    color2.rgb *= color2.a;
    color3.rgb *= color3.a;
    //vec4 color = color1 * 0.5 + color2 * 0.309017 + color3 * 0.190983;
    vec4 color = color2 + color3 * (1.0 - color2.a);
    color = color1 + color * (1.0 - color1.a);
    color.rgb /= color.a;
    out_Color = color;     
}