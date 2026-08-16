"""Build the authored Ash Creek architecture and export one Unity-ready GLB.

The town remains intentionally sparse and readable for the first-person demo, but
the landmark buildings use real layered construction instead of placeholder cubes:
board siding, trims, framed windows, porches, gabled roofs, tower masonry and
damaged details. Run with Blender 4.x in background mode.
"""

from __future__ import annotations

import math
import os
from pathlib import Path

import bpy
from mathutils import Vector


PROJECT = Path("/Users/leo/Documents/forgotten-trail-unity")
OUTPUT_DIR = PROJECT / "Assets/ForgottenTrail/Resources/Environment"
OUTPUT_GLB = OUTPUT_DIR / "AshCreek_Architecture.glb"
OUTPUT_BLEND = PROJECT / "Tools/Blender/AshCreek_Architecture.blend"
TEXTURE_DIR = PROJECT / "Assets/ForgottenTrail/Resources/Art/Textures"

WOOD_TEXTURE = TEXTURE_DIR / "Main File V1_1_Planks023A_512x512_Color.png"
WOOD_DARK_TEXTURE = TEXTURE_DIR / "Main File V1_1_Planks023A_512x512_Color_Black.png"
BRICK_TEXTURE = TEXTURE_DIR / "Main File V1_1_Bricks096_512x512_Color.png"


def clear_scene() -> None:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.meshes, bpy.data.curves, bpy.data.materials, bpy.data.cameras, bpy.data.lights):
        for datablock in list(datablocks):
            if datablock.users == 0:
                datablocks.remove(datablock)


def collection(name: str) -> bpy.types.Collection:
    root = bpy.data.collections.new(name)
    bpy.context.scene.collection.children.link(root)
    return root


def link_to(obj: bpy.types.Object, parent: bpy.types.Collection) -> bpy.types.Object:
    for coll in list(obj.users_collection):
        coll.objects.unlink(obj)
    parent.objects.link(obj)
    return obj


def material_color(name: str, color: tuple[float, float, float, float], roughness: float = 0.72,
                  metallic: float = 0.0, emission: tuple[float, float, float, float] | None = None) -> bpy.types.Material:
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = color
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = color
    bsdf.inputs["Roughness"].default_value = roughness
    bsdf.inputs["Metallic"].default_value = metallic
    if emission:
        bsdf.inputs["Emission Color"].default_value = emission
        bsdf.inputs["Emission Strength"].default_value = 1.8
    return mat


def material_texture(name: str, texture: Path, tint: tuple[float, float, float, float] | None = None,
                     roughness: float = 0.78) -> bpy.types.Material:
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    links = mat.node_tree.links
    nodes.clear()
    output = nodes.new("ShaderNodeOutputMaterial")
    bsdf = nodes.new("ShaderNodeBsdfPrincipled")
    image_node = nodes.new("ShaderNodeTexImage")
    image_node.image = bpy.data.images.load(str(texture), check_existing=True)
    image_node.interpolation = "Linear"
    image_node.extension = "REPEAT"
    bsdf.inputs["Roughness"].default_value = roughness
    if tint:
        multiply = nodes.new("ShaderNodeMixRGB")
        multiply.blend_type = "MULTIPLY"
        multiply.inputs[0].default_value = 0.72
        multiply.inputs[2].default_value = tint
        links.new(image_node.outputs["Color"], multiply.inputs[1])
        links.new(multiply.outputs[0], bsdf.inputs["Base Color"])
    else:
        links.new(image_node.outputs["Color"], bsdf.inputs["Base Color"])
    links.new(bsdf.outputs["BSDF"], output.inputs["Surface"])
    return mat


def add_box(parent: bpy.types.Collection, name: str, location: tuple[float, float, float],
            dimensions: tuple[float, float, float], material: bpy.types.Material,
            rotation: tuple[float, float, float] = (0.0, 0.0, 0.0), bevel: float = 0.035) -> bpy.types.Object:
    bpy.ops.mesh.primitive_cube_add(location=location, rotation=rotation)
    obj = link_to(bpy.context.object, parent)
    obj.name = name
    obj.dimensions = dimensions
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(material)
    if bevel > 0.0:
        modifier = obj.modifiers.new("Handworked edge", "BEVEL")
        modifier.width = min(bevel, min(dimensions) * 0.28)
        modifier.segments = 2
    return obj


def add_cylinder(parent: bpy.types.Collection, name: str, location: tuple[float, float, float],
                 radius: float, depth: float, material: bpy.types.Material,
                 rotation: tuple[float, float, float] = (0.0, 0.0, 0.0), vertices: int = 16) -> bpy.types.Object:
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth,
                                        location=location, rotation=rotation)
    obj = link_to(bpy.context.object, parent)
    obj.name = name
    obj.data.materials.append(material)
    modifier = obj.modifiers.new("Worn edge", "BEVEL")
    modifier.width = min(radius * 0.16, 0.035)
    modifier.segments = 2
    return obj


def add_gable(parent: bpy.types.Collection, name: str, center: tuple[float, float, float],
              width: float, height: float, thickness: float, material: bpy.types.Material) -> bpy.types.Object:
    cx, cy, cz = center
    half = width / 2.0
    verts = [
        (-half, 0.0, -thickness / 2.0), (half, 0.0, -thickness / 2.0), (0.0, height, -thickness / 2.0),
        (-half, 0.0, thickness / 2.0), (half, 0.0, thickness / 2.0), (0.0, height, thickness / 2.0),
    ]
    faces = [(0, 1, 2), (5, 4, 3), (0, 3, 4, 1), (1, 4, 5, 2), (2, 5, 3, 0)]
    mesh = bpy.data.meshes.new(name + "Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    parent.objects.link(obj)
    obj.location = (cx, cy, cz)
    obj.data.materials.append(material)
    bevel = obj.modifiers.new("Soft timber edge", "BEVEL")
    bevel.width = 0.035
    bevel.segments = 2
    return obj


def roof_panel(parent: bpy.types.Collection, name: str, center: tuple[float, float, float],
               span: float, depth: float, rise: float, material: bpy.types.Material) -> None:
    half = span / 2.0
    slope = math.sqrt(half * half + rise * rise)
    angle = math.atan2(rise, half)
    # The left/right panels share a ridge and overhang the front and rear walls.
    add_box(parent, name + "Left", (center[0] - half / 2.0, center[1] - rise / 2.0, center[2]),
            (slope + 0.18, 0.24, depth), material, (0.0, 0.0, angle), 0.025)
    add_box(parent, name + "Right", (center[0] + half / 2.0, center[1] - rise / 2.0, center[2]),
            (slope + 0.18, 0.24, depth), material, (0.0, 0.0, -angle), 0.025)
    add_box(parent, name + "Ridge", (center[0], center[1], center[2]), (0.22, 0.22, depth), material, bevel=0.025)


def add_vertical_siding(parent: bpy.types.Collection, prefix: str, center_x: float, front_z: float,
                        width: float, bottom: float, height: float, material: bpy.types.Material,
                        count: int) -> None:
    for index in range(count):
        x = center_x - width / 2.0 + (index + 0.5) * width / count
        add_box(parent, f"{prefix}_Board_{index:02d}", (x, bottom + height / 2.0, front_z - 0.14),
                (0.055, height - 0.1, 0.08), material, bevel=0.012)


def add_window(parent: bpy.types.Collection, prefix: str, center: tuple[float, float, float],
               width: float, height: float, facing: str, trim: bpy.types.Material,
               glass: bpy.types.Material, shutters: bool = True) -> None:
    x, y, z = center
    if facing == "front":
        add_box(parent, prefix + "_Glass", (x, y, z), (width, height, 0.08), glass, bevel=0.012)
        add_box(parent, prefix + "_Top", (x, y + height / 2.0, z - 0.07), (width + 0.24, 0.13, 0.17), trim)
        add_box(parent, prefix + "_Bottom", (x, y - height / 2.0, z - 0.07), (width + 0.24, 0.13, 0.17), trim)
        add_box(parent, prefix + "_Left", (x - width / 2.0, y, z - 0.07), (0.13, height, 0.17), trim)
        add_box(parent, prefix + "_Right", (x + width / 2.0, y, z - 0.07), (0.13, height, 0.17), trim)
        add_box(parent, prefix + "_MullionV", (x, y, z - 0.12), (0.075, height, 0.12), trim, bevel=0.01)
        add_box(parent, prefix + "_MullionH", (x, y, z - 0.12), (width, 0.075, 0.12), trim, bevel=0.01)
        if shutters:
            for side in (-1.0, 1.0):
                add_box(parent, prefix + ("_ShutterL" if side < 0 else "_ShutterR"),
                        (x + side * (width / 2.0 + 0.19), y, z - 0.01), (0.24, height + 0.14, 0.10), trim,
                        rotation=(0.0, side * 0.10, side * 0.08), bevel=0.018)
    else:
        add_box(parent, prefix + "_Glass", (x, y, z), (0.08, height, width), glass, bevel=0.012)
        add_box(parent, prefix + "_Top", (x, y + height / 2.0, z), (0.17, 0.13, width + 0.24), trim)
        add_box(parent, prefix + "_Bottom", (x, y - height / 2.0, z), (0.17, 0.13, width + 0.24), trim)
        add_box(parent, prefix + "_Mullion", (x, y, z), (0.12, height, 0.075), trim, bevel=0.01)


def add_door(parent: bpy.types.Collection, prefix: str, center: tuple[float, float, float],
             width: float, height: float, dark_wood: bpy.types.Material, trim: bpy.types.Material,
             opened: bool = False) -> None:
    x, y, z = center
    for side in (-1.0, 1.0):
        yaw = side * (0.15 if opened else 0.0)
        add_box(parent, prefix + ("_Left" if side < 0 else "_Right"),
                (x + side * width * 0.245, y, z), (width * 0.47, height, 0.14), dark_wood,
                rotation=(0.0, yaw, side * 0.015), bevel=0.025)
        add_box(parent, prefix + ("_TrimLeft" if side < 0 else "_TrimRight"),
                (x + side * width * 0.51, y, z - 0.08), (0.12, height + 0.24, 0.2), trim, bevel=0.018)
    add_box(parent, prefix + "_Lintel", (x, y + height / 2.0 + 0.14, z - 0.08), (width + 0.24, 0.18, 0.2), trim)


def add_porch(parent: bpy.types.Collection, prefix: str, center: tuple[float, float, float],
              width: float, depth: float, wood: bpy.types.Material, trim: bpy.types.Material,
              roof: bpy.types.Material) -> None:
    x, y, z = center
    add_box(parent, prefix + "_Deck", (x, y, z), (width, 0.18, depth), wood, bevel=0.025)
    for index in range(max(3, int(width / 1.8))):
        px = x - width / 2.0 + 0.45 + index * (width - 0.9) / max(1, int(width / 1.8) - 1)
        add_box(parent, f"{prefix}_DeckBoard_{index:02d}", (px, y + 0.12, z), (0.055, 0.08, depth - 0.16), trim, bevel=0.01)
    for px in (x - width / 2.0 + 0.3, x + width / 2.0 - 0.3):
        add_box(parent, prefix + "_Post", (px, y + 1.65, z + depth * 0.28), (0.22, 3.3, 0.22), trim)
    add_box(parent, prefix + "_Beam", (x, y + 3.22, z + depth * 0.28), (width + 0.3, 0.24, 0.25), trim)
    add_box(parent, prefix + "_Awning", (x, y + 3.04, z), (width + 0.35, 0.16, depth + 0.2), roof, rotation=(math.radians(-7), 0.0, 0.0), bevel=0.018)


def add_sign(parent: bpy.types.Collection, prefix: str, center: tuple[float, float, float],
             width: float, height: float, wood: bpy.types.Material, trim: bpy.types.Material) -> None:
    x, y, z = center
    add_box(parent, prefix + "_Board", (x, y, z), (width, height, 0.16), wood, bevel=0.045)
    add_box(parent, prefix + "_FrameTop", (x, y + height / 2.0, z - 0.1), (width + 0.16, 0.10, 0.17), trim, bevel=0.015)
    add_box(parent, prefix + "_FrameBottom", (x, y - height / 2.0, z - 0.1), (width + 0.16, 0.10, 0.17), trim, bevel=0.015)


def add_building_shell(parent: bpy.types.Collection, prefix: str, center: tuple[float, float, float],
                       width: float, depth: float, wall_height: float, wall: bpy.types.Material,
                       wood: bpy.types.Material, dark_wood: bpy.types.Material, trim: bpy.types.Material,
                       roof: bpy.types.Material, front_windows: int = 2, porch: bool = True,
                       gable: bool = True) -> None:
    x, y, z = center
    bottom = y + 0.32
    front = z - depth / 2.0
    back = z + depth / 2.0
    add_box(parent, prefix + "_Foundation", (x, y + 0.16, z), (width + 0.4, 0.32, depth + 0.4), trim, bevel=0.055)
    add_box(parent, prefix + "_Floor", (x, y + 0.42, z), (width, 0.18, depth), wood, bevel=0.02)
    add_box(parent, prefix + "_WallFront", (x, bottom + wall_height / 2.0, front), (width, wall_height, 0.22), wall, bevel=0.045)
    add_box(parent, prefix + "_WallBack", (x, bottom + wall_height / 2.0, back), (width, wall_height, 0.22), wall, bevel=0.045)
    add_box(parent, prefix + "_WallLeft", (x - width / 2.0, bottom + wall_height / 2.0, z), (0.22, wall_height, depth), wall, bevel=0.045)
    add_box(parent, prefix + "_WallRight", (x + width / 2.0, bottom + wall_height / 2.0, z), (0.22, wall_height, depth), wall, bevel=0.045)
    add_vertical_siding(parent, prefix + "_FrontSiding", x, front, width, bottom, wall_height, trim, max(8, int(width * 1.7)))
    add_box(parent, prefix + "_Fascia", (x, bottom + wall_height - 0.16, front - 0.12), (width + 0.24, 0.20, 0.28), trim)
    if gable:
        roof_base = bottom + wall_height
        rise = max(1.2, min(2.6, width * 0.18))
        add_gable(parent, prefix + "_FrontGable", (x, roof_base, front - 0.02), width + 0.18, rise, 0.24, wall)
        add_gable(parent, prefix + "_BackGable", (x, roof_base, back + 0.02), width + 0.18, rise, 0.24, wall)
        roof_panel(parent, prefix + "_Roof", (x, roof_base + rise, z), width + 0.8, depth + 0.55, rise, roof)
    else:
        add_box(parent, prefix + "_FlatRoof", (x, bottom + wall_height + 0.18, z), (width + 0.45, 0.28, depth + 0.45), roof, bevel=0.04)
    if porch:
        add_porch(parent, prefix + "_Porch", (x, y + 0.55, front - 0.85), width * 0.88, 1.55, wood, trim, roof)
    for index in range(front_windows):
        wx = x - width * 0.27 + index * (width * 0.54 / max(1, front_windows - 1)) if front_windows > 1 else x
        add_window(parent, f"{prefix}_FrontWindow_{index}", (wx, bottom + wall_height * 0.56, front - 0.18), 1.25, 1.55, "front", trim, GLASS)


def build_saloon(parent: bpy.types.Collection, mats: dict[str, bpy.types.Material]) -> None:
    x, z = -7.6, 11.75
    width, depth, height = 11.0, 9.5, 6.9
    wall_bottom = 0.32
    front = z - depth / 2.0
    back = z + depth / 2.0
    add_box(parent, "ARCH_Saloon_Foundation", (x, 0.16, z), (width + 0.5, 0.32, depth + 0.5), mats["stone"], bevel=0.06)
    # Keep the ground-floor entrance physically open so the authored saloon
    # interior remains readable when the first-person player reaches it.
    ground_center_y = wall_bottom + 1.45
    ground_front = front - 0.02
    add_box(parent, "ARCH_Saloon_GroundBack", (x, ground_center_y, back), (width, 2.9, 0.22), mats["wood"], bevel=0.05)
    add_box(parent, "ARCH_Saloon_GroundLeft", (x - width / 2.0, ground_center_y, z), (0.22, 2.9, depth), mats["wood"], bevel=0.05)
    add_box(parent, "ARCH_Saloon_GroundRight", (x + width / 2.0, ground_center_y, z), (0.22, 2.9, depth), mats["wood"], bevel=0.05)
    door_center_x, door_width = x - 1.5, 2.3
    segment_width = (width - door_width) / 2.0
    add_box(parent, "ARCH_Saloon_GroundFrontLeft", (x - (door_width + segment_width) / 2.0, ground_center_y, ground_front),
            (segment_width, 2.9, 0.22), mats["wood"], bevel=0.05)
    add_box(parent, "ARCH_Saloon_GroundFrontRight", (x + (door_width + segment_width) / 2.0, ground_center_y, ground_front),
            (segment_width, 2.9, 0.22), mats["wood"], bevel=0.05)
    add_box(parent, "ARCH_Saloon_GroundFrontHeader", (door_center_x, 2.99, ground_front),
            (door_width + 0.2, 0.46, 0.22), mats["trim"], bevel=0.03)
    add_box(parent, "ARCH_Saloon_UpperBody", (x, wall_bottom + 4.45, z), (width - 0.22, 3.0, depth - 0.2), mats["wood_dark"], bevel=0.05)
    add_vertical_siding(parent, "ARCH_Saloon_Front", x, front, width, wall_bottom, height, mats["trim"], 22)
    add_box(parent, "ARCH_Saloon_UpperFascia", (x, wall_bottom + 3.0, front - 0.12), (width + 0.22, 0.20, 0.28), mats["trim"])
    add_box(parent, "ARCH_Saloon_RoofLine", (x, wall_bottom + height + 0.03, front - 0.12), (width + 0.26, 0.22, 0.28), mats["trim"])
    rise = 2.45
    roof_panel(parent, "ARCH_Saloon_Roof", (x, wall_bottom + height + rise, z), width + 1.0, depth + 0.75, rise, mats["roof"])
    add_gable(parent, "ARCH_Saloon_FrontGable", (x, wall_bottom + height, front - 0.01), width + 0.3, rise, 0.28, mats["wood_dark"])
    add_gable(parent, "ARCH_Saloon_BackGable", (x, wall_bottom + height, back + 0.01), width + 0.3, rise, 0.28, mats["wood_dark"])
    add_porch(parent, "ARCH_Saloon_Porch", (x, 0.55, front - 0.95), width + 0.25, 1.8, mats["wood"], mats["trim"], mats["roof"])
    add_door(parent, "ARCH_Saloon_Door", (x - 1.5, 1.55, front - 1.90), 2.3, 2.45, mats["wood_dark"], mats["trim"], opened=True)
    add_window(parent, "ARCH_Saloon_WindowLeft", (x - 4.1, 1.72, front - 0.2), 1.55, 1.6, "front", mats["trim"], GLASS)
    add_window(parent, "ARCH_Saloon_WindowRight", (x + 2.75, 1.72, front - 0.2), 1.55, 1.6, "front", mats["trim"], GLASS)
    add_window(parent, "ARCH_Saloon_UpperWindowLeft", (x - 3.75, 4.95, front - 0.2), 1.55, 1.55, "front", mats["trim"], GLASS)
    add_window(parent, "ARCH_Saloon_UpperWindowRight", (x + 2.55, 4.95, front - 0.2), 1.55, 1.55, "front", mats["trim"], GLASS)
    add_window(parent, "ARCH_Saloon_SideWindow", (x + width / 2.0 + 0.2, 4.85, z + 1.8), 1.35, 1.4, "side", mats["trim"], GLASS, shutters=False)
    add_sign(parent, "ARCH_Saloon_Sign", (x - 1.5, 4.05, front - 2.02), 4.8, 1.0, mats["wood_dark"], mats["trim"])
    add_box(parent, "ARCH_Saloon_BloodBoard", (x - 4.55, 1.02, front - 0.19), (0.75, 0.28, 0.035), mats["blood"], rotation=(0.0, 0.0, math.radians(-7)), bevel=0.008)
    add_box(parent, "ARCH_Saloon_DamagedBoard", (x + 4.55, 3.2, front - 0.18), (0.12, 1.35, 0.05), mats["wood_dark"], rotation=(0.0, math.radians(12), math.radians(-13)), bevel=0.012)


def build_church(parent: bpy.types.Collection, mats: dict[str, bpy.types.Material]) -> None:
    x, z = 7.0, 27.5
    width, depth, wall_height = 8.6, 14.4, 7.1
    front = z - depth / 2.0
    back = z + depth / 2.0
    bottom = 0.32
    add_box(parent, "ARCH_Church_Foundation", (x, 0.16, z), (width + 0.45, 0.32, depth + 0.45), mats["stone"], bevel=0.06)
    body_y = bottom + wall_height / 2.0
    body_z = z + 0.25
    add_box(parent, "ARCH_Church_Back", (x, body_y, back), (width, wall_height, 0.22), mats["plaster"], bevel=0.05)
    add_box(parent, "ARCH_Church_Left", (x - width / 2.0, body_y, body_z), (0.22, wall_height, depth - 0.5), mats["plaster"], bevel=0.05)
    add_box(parent, "ARCH_Church_Right", (x + width / 2.0, body_y, body_z), (0.22, wall_height, depth - 0.5), mats["plaster"], bevel=0.05)
    door_width, door_top = 2.3, 2.855
    side_width = (width - door_width) / 2.0
    add_box(parent, "ARCH_Church_FrontLeft", (x - (door_width + side_width) / 2.0, body_y, front),
            (side_width, wall_height, 0.22), mats["plaster"], bevel=0.05)
    add_box(parent, "ARCH_Church_FrontRight", (x + (door_width + side_width) / 2.0, body_y, front),
            (side_width, wall_height, 0.22), mats["plaster"], bevel=0.05)
    add_box(parent, "ARCH_Church_FrontHeader", (x, (door_top + bottom + wall_height) / 2.0, front),
            (door_width + 0.2, bottom + wall_height - door_top, 0.22), mats["plaster"], bevel=0.05)
    add_box(parent, "ARCH_Church_FrontFascia", (x, bottom + wall_height - 0.16, front - 0.18), (width + 0.24, 0.2, 0.3), mats["stone"])
    rise = 2.6
    add_gable(parent, "ARCH_Church_FrontGable", (x, bottom + wall_height, front - 0.05), width + 0.22, rise, 0.3, mats["plaster"])
    add_gable(parent, "ARCH_Church_BackGable", (x, bottom + wall_height, back + 0.05), width + 0.22, rise, 0.3, mats["plaster"])
    roof_panel(parent, "ARCH_Church_Roof", (x, bottom + wall_height + rise, z + 0.25), width + 0.8, depth + 0.75, rise, mats["roof"])
    add_door(parent, "ARCH_Church_Door", (x, 1.58, front - 0.25), 2.3, 2.55, mats["wood_dark"], mats["stone"], opened=True)
    add_window(parent, "ARCH_Church_WindowLeft", (x - 2.25, 3.25, front - 0.22), 1.15, 2.45, "front", mats["stone"], GLASS, shutters=False)
    add_window(parent, "ARCH_Church_WindowRight", (x + 2.25, 3.25, front - 0.22), 1.15, 2.45, "front", mats["stone"], GLASS, shutters=False)
    for index, wx in enumerate((-2.5, 0.0, 2.5)):
        add_window(parent, f"ARCH_Church_SideWindow_{index}", (x - width / 2.0 - 0.14, 3.55, z + wx), 1.1, 2.15, "side", mats["stone"], GLASS, shutters=False)
    # Bell tower: a solid masonry base, an open dark belfry and a steep cap.
    tower_x, tower_z = x, front - 0.55
    add_box(parent, "ARCH_Church_Tower", (tower_x, 5.3, tower_z), (3.45, 9.9, 3.5), mats["brick"], bevel=0.055)
    add_box(parent, "ARCH_Church_Tower_Belfry", (tower_x, 9.1, tower_z - 0.02), (3.05, 1.8, 3.1), mats["dark_stone"], bevel=0.04)
    add_window(parent, "ARCH_Church_TowerBellOpening", (tower_x, 9.1, tower_z - 1.62), 1.35, 1.2, "front", mats["stone"], mats["black"], shutters=False)
    roof_panel(parent, "ARCH_Church_TowerRoof", (tower_x, 10.75, tower_z), 4.1, 4.1, 2.45, mats["roof"])
    add_cylinder(parent, "ARCH_Church_Bell", (tower_x, 9.05, tower_z), 0.42, 0.58, mats["metal"], rotation=(math.pi / 2.0, 0.0, 0.0), vertices=20)
    add_box(parent, "ARCH_Church_CrossVertical", (tower_x, 13.0, tower_z - 0.03), (0.16, 1.1, 0.16), mats["metal"], bevel=0.02)
    add_box(parent, "ARCH_Church_CrossHorizontal", (tower_x, 13.05, tower_z - 0.03), (0.65, 0.16, 0.16), mats["metal"], bevel=0.02)
    add_box(parent, "ARCH_Church_BloodCloth", (x + 3.35, 0.78, front + 0.5), (0.6, 0.07, 0.25), mats["blood"], rotation=(0.0, 0.0, math.radians(-18)), bevel=0.008)


def build_station(parent: bpy.types.Collection, mats: dict[str, bpy.types.Material]) -> None:
    x, z = 0.0, 36.25
    width, depth, wall_height = 10.5, 10.7, 7.0
    front = z - depth / 2.0
    bottom = 0.32
    add_box(parent, "ARCH_Station_Foundation", (x, 0.16, z), (width + 0.5, 0.32, depth + 0.5), mats["stone"], bevel=0.06)
    station_body_y = bottom + wall_height / 2.0
    add_box(parent, "ARCH_Station_Back", (x, station_body_y, z + depth / 2.0), (width, wall_height, 0.22), mats["wood_dark"], bevel=0.05)
    add_box(parent, "ARCH_Station_Left", (x - width / 2.0, station_body_y, z), (0.22, wall_height, depth), mats["wood_dark"], bevel=0.05)
    add_box(parent, "ARCH_Station_Right", (x + width / 2.0, station_body_y, z), (0.22, wall_height, depth), mats["wood_dark"], bevel=0.05)
    station_door_width, station_door_top = 2.0, 2.8
    station_side_width = (width - station_door_width) / 2.0
    add_box(parent, "ARCH_Station_FrontLeft", (x - (station_door_width + station_side_width) / 2.0, station_body_y, front),
            (station_side_width, wall_height, 0.22), mats["wood_dark"], bevel=0.05)
    add_box(parent, "ARCH_Station_FrontRight", (x + (station_door_width + station_side_width) / 2.0, station_body_y, front),
            (station_side_width, wall_height, 0.22), mats["wood_dark"], bevel=0.05)
    add_box(parent, "ARCH_Station_FrontHeader", (x, (station_door_top + bottom + wall_height) / 2.0, front),
            (station_door_width + 0.2, bottom + wall_height - station_door_top, 0.22), mats["wood_dark"], bevel=0.05)
    add_vertical_siding(parent, "ARCH_Station_Front", x, front, width, bottom, wall_height, mats["trim"], 20)
    add_box(parent, "ARCH_Station_Fascia", (x, bottom + wall_height - 0.15, front - 0.14), (width + 0.25, 0.2, 0.28), mats["trim"])
    rise = 2.2
    add_gable(parent, "ARCH_Station_FrontGable", (x, bottom + wall_height, front - 0.02), width + 0.28, rise, 0.26, mats["wood_dark"])
    roof_panel(parent, "ARCH_Station_Roof", (x, bottom + wall_height + rise, z), width + 0.9, depth + 0.7, rise, mats["roof"])
    add_porch(parent, "ARCH_Station_Porch", (x, 0.55, front - 0.88), width * 0.86, 1.6, mats["wood"], mats["trim"], mats["roof"])
    add_door(parent, "ARCH_Station_Door", (x, 1.55, front - 1.75), 2.0, 2.5, mats["wood"], mats["trim"])
    add_window(parent, "ARCH_Station_WindowLeft", (x - 3.65, 1.8, front - 0.19), 1.45, 1.65, "front", mats["trim"], GLASS)
    add_window(parent, "ARCH_Station_WindowRight", (x + 3.65, 1.8, front - 0.19), 1.45, 1.65, "front", mats["trim"], GLASS)
    add_window(parent, "ARCH_Station_UpperWindowLeft", (x - 2.4, 5.1, front - 0.19), 1.4, 1.45, "front", mats["trim"], GLASS)
    add_window(parent, "ARCH_Station_UpperWindowRight", (x + 2.4, 5.1, front - 0.19), 1.4, 1.45, "front", mats["trim"], GLASS)
    # Sheriff office sign and a small balcony to break the flat silhouette.
    add_sign(parent, "ARCH_Station_Sign", (x, 3.9, front - 1.9), 3.9, 0.9, mats["wood"], mats["trim"])
    add_box(parent, "ARCH_Station_Balcony", (x, 3.35, front - 2.15), (4.4, 0.16, 0.9), mats["wood"], bevel=0.02)
    add_box(parent, "ARCH_Station_BalconyRail", (x, 4.0, front - 2.48), (4.4, 0.16, 0.14), mats["trim"], bevel=0.015)
    for bx in (-1.9, -0.95, 0.0, 0.95, 1.9):
        add_box(parent, "ARCH_Station_BalconyPost", (x + bx, 3.7, front - 2.48), (0.1, 0.75, 0.1), mats["trim"], bevel=0.012)
    # Bars are physical meshes, not painted lines.
    for index in range(5):
        add_box(parent, f"ARCH_Station_Bar_{index}", (x - 3.65 + index * 0.18, 1.8, front - 0.27),
                (0.055, 1.55, 0.08), mats["metal"], bevel=0.01)
    add_box(parent, "ARCH_Station_DamagedPlank", (x + 4.3, 3.0, front - 0.2), (0.14, 1.45, 0.05), mats["wood"],
            rotation=(0.0, math.radians(-9), math.radians(15)), bevel=0.012)


def build_backdrop(parent: bpy.types.Collection, mats: dict[str, bpy.types.Material]) -> None:
    buildings = [
        ("ARCH_BoardingHouse", (-15.0, 0.0, 7.0), 7.2, 5.5, 4.9, mats["plaster"], 7, True, 17.0),
        ("ARCH_Mercantile", (15.0, 0.0, 10.7), 8.6, 5.9, 5.2, mats["brick"], 8, True, -18.0),
        ("ARCH_Blacksmith", (-15.2, 0.0, 25.8), 8.0, 6.1, 5.1, mats["wood_dark"], 7, False, 12.0),
        ("ARCH_DoctorHouse", (15.1, 0.0, 29.3), 7.9, 6.1, 5.4, mats["plaster"], 7, True, -13.0),
        ("ARCH_NorthCabin", (-12.4, 0.0, 44.2), 7.2, 5.2, 4.6, mats["wood"], 7, False, 10.0),
        ("ARCH_EastCabin", (13.5, 0.0, 44.8), 7.4, 5.4, 4.8, mats["wood_dark"], 7, False, -14.0),
    ]
    for name, center, width, depth, height, wall, windows, porch, yaw in buildings:
        before = set(bpy.context.scene.objects)
        add_building_shell(parent, name, center, width, depth, height, wall, mats["wood"], mats["wood_dark"],
                           mats["trim"], mats["roof"], front_windows=2 if windows > 6 else 1, porch=porch, gable=True)
        created = [obj for obj in bpy.context.scene.objects if obj not in before and obj.name.startswith(name)]
        pivot = bpy.data.objects.new(name + "_Pivot", None)
        parent.objects.link(pivot)
        pivot.location = center
        pivot.rotation_euler[2] = math.radians(yaw)
        for obj in created:
            obj.parent = pivot
            obj.location = Vector(obj.location) - Vector(center)
        # Keep the pivot in world space at the requested location after reparenting.
        pivot.location = center


def make_materials() -> dict[str, bpy.types.Material]:
    wood = material_texture("AshCreek_WoodBoards", WOOD_TEXTURE)
    wood_dark = material_texture("AshCreek_DarkWood", WOOD_DARK_TEXTURE)
    brick = material_texture("AshCreek_Brick", BRICK_TEXTURE)
    trim = material_color("AshCreek_Trim", (0.16, 0.07, 0.028, 1.0), 0.64)
    roof = material_color("AshCreek_TarredRoof", (0.022, 0.026, 0.028, 1.0), 0.9)
    plaster = material_color("AshCreek_AgedPlaster", (0.30, 0.245, 0.19, 1.0), 0.89)
    stone = material_color("AshCreek_CutStone", (0.23, 0.24, 0.23, 1.0), 0.92)
    dark_stone = material_color("AshCreek_BelfryShadow", (0.035, 0.038, 0.038, 1.0), 0.98)
    metal = material_color("AshCreek_Iron", (0.055, 0.06, 0.06, 1.0), 0.7, metallic=0.82)
    glass = material_color("AshCreek_SmokyWindow", (0.018, 0.028, 0.03, 1.0), 0.22,
                           emission=(0.10, 0.028, 0.008, 1.0))
    black = material_color("AshCreek_BelfryOpening", (0.003, 0.003, 0.003, 1.0), 1.0)
    blood = material_color("AshCreek_DriedBlood", (0.19, 0.015, 0.008, 1.0), 0.82)
    global GLASS
    GLASS = glass
    return {
        "wood": wood, "wood_dark": wood_dark, "brick": brick, "trim": trim, "roof": roof,
        "plaster": plaster, "stone": stone, "dark_stone": dark_stone, "metal": metal,
        "glass": glass, "black": black, "blood": blood,
    }


def main() -> None:
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    OUTPUT_BLEND.parent.mkdir(parents=True, exist_ok=True)
    clear_scene()
    architecture = collection("AshCreek_Architecture")
    mats = make_materials()
    build_saloon(architecture, mats)
    build_church(architecture, mats)
    build_station(architecture, mats)
    build_backdrop(architecture, mats)

    # Export only authored meshes. Empty pivots remain in the hierarchy and make
    # the imported landmark groups easy to inspect in Unity.
    bpy.ops.object.select_all(action="DESELECT")
    for obj in architecture.objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = next((obj for obj in architecture.objects if obj.type == "MESH"), None)
    bpy.ops.wm.save_as_mainfile(filepath=str(OUTPUT_BLEND))
    bpy.ops.export_scene.gltf(
        filepath=str(OUTPUT_GLB),
        export_format="GLB",
        use_selection=True,
        export_apply=True,
        export_materials="EXPORT",
    )
    print(f"Exported {OUTPUT_GLB}")
    print(f"Saved source {OUTPUT_BLEND}")


if __name__ == "__main__":
    main()
