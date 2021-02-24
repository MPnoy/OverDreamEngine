#version 420

in vec2 fragUV;
out vec4 out_Color;

uniform sampler2D BlitTex;
uniform vec4 BlitRect;

uniform float ScaleX = 4.0;
uniform float Speed = 1.0;
uniform vec4 Rectangle = vec4(0.0, 0.0, 1.0, 1.0);
uniform float SinTime = 0.0;
uniform float CosTime = 0.0;


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

float calc(float k, float y)
{
    return (k * 5 + 0.5) - (y - 0.5) * 5;
}


void main()
{
    vec2 blitUV = mix(BlitRect.xy, BlitRect.zw, fragUV);
    vec4 color = texture(BlitTex, blitUV);
    vec2 scaledUV = mix(vec2(Rectangle.x, Rectangle.y), vec2(Rectangle.z, Rectangle.w), fragUV);
    float iter1 = ScaleX;
    float iter2 = iter1;
    float k = -1.0;
    float calced1 = calc(1.0 / ScaleX, scaledUV.y);
    if (calced1 > 0.0)
    {
        k = 0.0;
        for (int i = 0; i < 4; i++)
        {
            iter1 *= 2.0;
            iter2 *= 2.0;
            k += (snoise(vec3(scaledUV.x * iter1, SinTime * Speed * 1000.0, CosTime * Speed * 1000.0))) / iter2;
        }
        k /= 1.0;
    }
    float k1 = 1 - clamp(calc(k, scaledUV.y), 0.0, 1.0);
    float k2 = 1 - clamp((k + 0.5) - (scaledUV.y - 0.5), 0.0, 1.0);
    float r = mix(0.2, 1.0, k1);
    float g = mix(0.2, 1.0, k1);
    float b = cos((k2 - 1.0) * 3.141592 / 2);
    b = 1.0 - (1.0 - b) * (1.0 - b);
    r = min(r, b);
    g = min(r, g);
    vec4 mask = vec4(r, g, b, 1);
    out_Color = color * mask;     
}