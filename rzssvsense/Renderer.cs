using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using ImGuiNET;

namespace rzssvsense
{
    public class Renderer : Overlay
    {
        public Vector2 overlaySize = new Vector2();
        Vector2 windowLocation = new Vector2(0, 0);
        public List<Entity> entitiesCopy = new List<Entity>();
        public Entity localPlayerCopy = new Entity();
        ImDrawListPtr drawList;
        Vector4 teamColor = new Vector4(0.128f, 1, 0, 1);
        Vector4 enemyColor = new Vector4(1, 0, 0, 1);
        float boneThickness = 4;
        public bool thirdPerson = false;
        public bool bunnyHopping = false;
        public int fov = 90;
        public bool radar = false;
        public bool antiRecoil = false;
        public bool aimbot = false;
        public bool silent = false;
        public bool aimOnTeam = false;
        public bool triggerBot = false;
        public bool esp = false;
        public float aimbotFov = 50;
        public Vector4 circleColor = new Vector4(1, 1, 1, 1);
        protected override void Render()
        {
            ImGui.Begin("fishware");
            ImGui.Checkbox("Bhop", ref bunnyHopping);
            ImGui.Checkbox("Third Person (broken)", ref thirdPerson);
            ImGui.SliderInt("FOV", ref fov, 58, 140);
            ImGui.Checkbox("Radar (broken)", ref radar);
            ImGui.Checkbox("Disable Recoil(buggy)", ref antiRecoil);
            ImGui.Checkbox("Aimbot", ref aimbot);
            ImGui.Checkbox("Silent Aim(buggy)", ref silent);
            ImGui.Checkbox("Aim On Teammates", ref aimOnTeam);
            ImGui.SliderFloat("Aimbot FOV", ref aimbotFov, 10, 300);
            if (ImGui.CollapsingHeader("FOV Colour"))
                ImGui.ColorPicker4("##FovColour", ref circleColor);
            ImGui.Checkbox("Triggerbot", ref triggerBot);
            ImGui.Checkbox("Bone ESP", ref esp);
            ImGui.SliderFloat("Bone Thickness", ref boneThickness, 4, 500);
            if (ImGui.CollapsingHeader("Team Colour"))
            {
                ImGui.ColorPicker4("##TeamColour", ref teamColor);
            }
            if (ImGui.CollapsingHeader("Enemy Colour"))
            {
                ImGui.ColorPicker4("##EnemyColour", ref enemyColor);
            }
            if (esp)
            {
                DrawOverlay();
                DrawSkeletons();
            }
            DrawOverlay();
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            drawList.AddCircle(new Vector2(overlaySize.X / 2, overlaySize.Y / 2), aimbotFov, ImGui.ColorConvertFloat4ToU32(circleColor));
        }
        void DrawSkeletons()
        {
            if (entitiesCopy.Count == 0 || entitiesCopy == null)
                return;
            List<Entity> tempEntities = new List<Entity>(entitiesCopy).ToList();
            drawList = ImGui.GetWindowDrawList();
            uint uintColor;
            foreach (Entity entity in tempEntities)
            {
                if (entity == null) continue;
                uintColor = localPlayerCopy.team == entity.team ? ImGui.ColorConvertFloat4ToU32(teamColor) : ImGui.ColorConvertFloat4ToU32(enemyColor);
                if (entity.bones2d[2].X > 0 && entity.bones2d[2].Y > 0 && entity.bones2d[2].X < overlaySize.X && entity.bones2d[2].Y < overlaySize.Y)
                {
                    float currentBoneThickness = boneThickness / entity.distance;
                    drawList.AddLine(entity.bones2d[1], entity.bones2d[2], uintColor, currentBoneThickness);
                    drawList.AddLine(entity.bones2d[1], entity.bones2d[3], uintColor, currentBoneThickness);
                    drawList.AddLine(entity.bones2d[1], entity.bones2d[6], uintColor, currentBoneThickness);
                    drawList.AddLine(entity.bones2d[1], entity.bones2d[0], uintColor, currentBoneThickness);
                    drawList.AddLine(entity.bones2d[3], entity.bones2d[4], uintColor, currentBoneThickness);
                    drawList.AddLine(entity.bones2d[6], entity.bones2d[7], uintColor, currentBoneThickness);
                    drawList.AddLine(entity.bones2d[4], entity.bones2d[5], uintColor, currentBoneThickness);
                    drawList.AddLine(entity.bones2d[7], entity.bones2d[8], uintColor, currentBoneThickness);
                    drawList.AddLine(entity.bones2d[0], entity.bones2d[9], uintColor, currentBoneThickness);
                    drawList.AddLine(entity.bones2d[0], entity.bones2d[11], uintColor, currentBoneThickness);
                    drawList.AddLine(entity.bones2d[9], entity.bones2d[10], uintColor, currentBoneThickness);
                    drawList.AddLine(entity.bones2d[11], entity.bones2d[12], uintColor, currentBoneThickness);
                    drawList.AddCircle(entity.bones2d[2], 3 + currentBoneThickness, uintColor);
                }
            }
        }
        void DrawOverlay()
        {
            ImGui.SetNextWindowSize(overlaySize);
            ImGui.SetNextWindowPos(windowLocation);
            ImGui.Begin("overlay", ImGuiWindowFlags.NoDecoration
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoBringToFrontOnFocus
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoInputs
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse
                );
        }
    }
}
