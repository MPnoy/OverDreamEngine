#version 420

in vec4 coord;
in vec4 color;
out vec2 fragUV;
out vec4 fragColor;

void main()
{
    gl_Position = vec4(coord.xy, 0.0, 1.0);
    fragUV = coord.zw;
    fragColor = color;
}
