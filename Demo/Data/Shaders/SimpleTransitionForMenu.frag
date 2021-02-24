#version 420

uniform sampler2D Tex1;
uniform sampler2D Tex2;

uniform float CrossFade = 0.0;
uniform float Alpha = 1.0;

in vec2 fragUV;
out vec4 out_Color;

void main()
{
    vec4 o1 = texture(Tex1, fragUV);
    vec4 o2 = texture(Tex2, fragUV);
    o1.rgb *= o1.a;
    o2.rgb *= o2.a;
    vec4 o = mix(o1, o2, CrossFade);
    o.rgb /= o.a;
    o.a *= Alpha;
    out_Color = o;
}