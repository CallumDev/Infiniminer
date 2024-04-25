@vertex
@include(includes/camera.inc)
#define FADE_DISTANCE (64.0)

uniform mat4x4 World;

in vec3 vertex_position;
in vec2 vertex_texture1;
in float vertex_color;

out vec2 tex;
out float shade;
out float fade;

void main()
{
    vec4 pos = (ViewProjection * World) * vec4(vertex_position, 1.0);
    gl_Position = pos;
    tex = vertex_texture1;
    shade = vertex_color;
    fade = clamp(pos.z, 0.0, FADE_DISTANCE) / FADE_DISTANCE;
}

@fragment
uniform vec4 LODColor;
uniform sampler2D Texture;

in vec2 tex;
in float shade;
in float fade;

out vec4 out_color;

void main()
{
    vec4 texColor = texture(Texture, tex);
    out_color = vec4(mix(texColor.rgb, LODColor.rgb, fade) * shade, texColor.a);
}
