#version 420

uniform sampler2D PrevTex;
uniform vec4 PrevRect;

in vec2 fragUV;
out vec4 out_Color;

void main() 
{
    out_Color = texture(PrevTex, mix(PrevRect.xy, PrevRect.zw, fragUV));
}
