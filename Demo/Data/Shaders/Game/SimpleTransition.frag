#version 420

uniform sampler2D Tex1Tex;
uniform sampler2D Tex2Tex;
uniform vec4 Tex1Rect;
uniform vec4 Tex2Rect;

uniform float CrossFade;
uniform float Alpha;
uniform float AlphaLoading;

in vec2 fragUV;
out vec4 out_Color;

void main()
{
    vec4 o1 = texture(Tex1Tex, mix(Tex1Rect.xy, Tex1Rect.zw, fragUV));
    vec4 o2 = texture(Tex2Tex, mix(Tex2Rect.xy, Tex2Rect.zw, fragUV));
    o1.rgb *= o1.a;
    o2.rgb *= o2.a;
    vec4 o = mix(o1, o2, CrossFade);
    o.rgb /= o.a;
    o.a *= Alpha * AlphaLoading;
    out_Color = o;
}