#version 420

uniform sampler2D Tex1Tex;
uniform sampler2D Tex2Tex;
uniform vec4 Tex1Rect;
uniform vec4 Tex2Rect;

uniform sampler2D Mask;
uniform float CrossFade;
uniform float Alpha;
uniform float AlphaLoading;

in vec2 fragUV;
out vec4 out_Color;

void main()
{
    vec4 o1 = texture(Tex1Tex, mix(Tex1Rect.xy, Tex1Rect.zw, fragUV));
    vec4 o2 = texture(Tex2Tex, mix(Tex2Rect.xy, Tex2Rect.zw, fragUV));
    vec4 m = texture(Mask, fragUV) * 0.9999;
    vec4 o = o1;
    for (int i = 0; i < 4; i++)
    {
        if (CrossFade > m[i])
        {
            o[i] = o2[i];
            o[3] = o2[3];
        }
    }
    o[3] *= Alpha * AlphaLoading;
    out_Color = o;
}