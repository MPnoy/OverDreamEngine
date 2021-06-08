#version 420

uniform sampler2D blitInput;

uniform vec4 color;

in vec2 fragUV;
out vec4 out_Color;

void main()
{
    vec4 ret = texture(blitInput, fragUV) * color;
    ret.rgb *= ret.a;
    out_Color = ret;
    
}
