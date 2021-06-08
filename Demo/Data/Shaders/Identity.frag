#version 420

uniform sampler2D blitInput;

in vec2 fragUV;
out vec4 out_Color;

void main()
{
    out_Color = texture(blitInput, fragUV);
}
