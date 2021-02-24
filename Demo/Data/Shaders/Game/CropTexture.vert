#version 420

uniform vec4 _Rect;

in vec3 vertex_position;
in vec2 uv;

out vec2 fragUV;

void main() 
{
    gl_Position = vec4(vertex_position, 1.0);
    fragUV.x = mix(_Rect.x, _Rect.z, uv.x);
    fragUV.y = mix(_Rect.y, _Rect.w, uv.y);
}
