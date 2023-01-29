shader_type canvas_item;
render_mode unshaded;

uniform float k = 0;

void fragment()
{
    COLOR = texture(TEXTURE, UV);
    float l = COLOR.r * 0.2126 + COLOR.g * 0.7152 + COLOR.b * 0.0722;
    COLOR.rgb = vec3(COLOR.r-(COLOR.r-l)*k, COLOR.g-(COLOR.g-l)*k, COLOR.b-(COLOR.b-l)*k);
}