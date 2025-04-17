using System.Numerics;
using System.Runtime.InteropServices;
using rzssvsense;
using Swed64;

Renderer renderer = new Renderer();
Swed swed = new Swed("cs2");
Thread renderThread = new Thread(renderer.Start().Wait);
renderThread.Start();
Thread bhopThread = new Thread(new ThreadStart(bHop.BHop));
bhopThread.Start();
Reader reader = new Reader(swed);

IntPtr client = swed.GetModuleBase("client.dll");

const int Hotkey = 0x06;


List<Entity> entities = new List<Entity>();
Entity localPlayer = new Entity();
Vector2 screen = new Vector2(2560, 1440);

renderer.overlaySize = screen;


while (true)
{
    // third person
    if (renderer.thirdPerson)
        swed.WriteUInt(client + 0x1A93240, 256);
    else
        swed.WriteUInt(client + 0x1A93240, 0);

    //fov
    uint desiredFov = (uint)renderer.fov;
    IntPtr localPlayerPawn = swed.ReadPointer(client, Offsets.dwLocalPlayerPawn);
    IntPtr cameraServices = swed.ReadPointer(localPlayerPawn, Offsets.m_pCameraServices);
    uint currentFov = swed.ReadUInt(cameraServices + Offsets.m_iFOV);
    bool isScoped = swed.ReadBool(localPlayerPawn, Offsets.m_bIsScoped);
    Vector3 velocity = swed.ReadVec(localPlayerPawn, Offsets.m_vecAbsVelocity);
    int speed = (int)Math.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y + velocity.Z * velocity.Z);

    if (!isScoped && currentFov != desiredFov)
    {
        swed.WriteUInt(cameraServices + Offsets.m_iFOV, desiredFov);
    }

    //radar + aimbot + silent aim
    entities.Clear();
    localPlayer.pawnAddress = swed.ReadPointer(client, Offsets.dwLocalPlayerPawn);
    localPlayer.team = swed.ReadInt(localPlayer.pawnAddress, Offsets.m_iTeamNum);
    localPlayer.origin = swed.ReadVec(localPlayer.pawnAddress, Offsets.m_vOldOrigin);
    localPlayer.view = swed.ReadVec(localPlayer.pawnAddress, Offsets.m_vecViewOffset);
    int entIndex = swed.ReadInt(localPlayerPawn, Offsets.m_iIDEntIndex);
    IntPtr entityList = swed.ReadPointer(client, Offsets.dwEntityList);
    IntPtr listEntry = swed.ReadPointer(entityList, 0x10);
    for (int i = 0; i < 64; i++)
    {
        if (listEntry == IntPtr.Zero)
            continue;
        IntPtr controller = swed.ReadPointer(listEntry, i * 0x78);
        if (controller == IntPtr.Zero)
            continue;
        int pawnHandle = swed.ReadInt(controller, Offsets.m_hPlayerPawn);
        if (pawnHandle == 0)
            continue;
        IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);
        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));
        if (currentPawn == localPlayer.pawnAddress)
            continue;
        IntPtr sceneNode = swed.ReadPointer(currentPawn, Offsets.m_pGameSceneNode);
        IntPtr boneMatrix = swed.ReadPointer(sceneNode, Offsets.m_modelState + 0x80);
        ViewMatrix viewMatrix = reader.readMatrix(client + Offsets.dwViewMatrix);
        string name = swed.ReadString(controller, Offsets.m_iszPlayerName, 16);
        bool spotted = swed.ReadBool(currentPawn, Offsets.m_entitySpottedState + Offsets.m_bSpotted);
        int health = swed.ReadInt(currentPawn, Offsets.m_iHealth);
        int team = swed.ReadInt(currentPawn, Offsets.m_iTeamNum);
        uint lifeState = swed.ReadUInt(currentPawn, Offsets.m_lifeState);
        if (lifeState != 256)
            continue;
        if (team == localPlayer.team && !renderer.aimOnTeam)
            continue;
        if (renderer.radar)
            swed.WriteBool(currentPawn, Offsets.m_entitySpottedState + Offsets.m_bSpotted, true);
        Entity entity = new Entity();
        entity.pawnAddress = currentPawn;
        entity.controller = controller;
        entity.health = health;
        entity.lifeState = lifeState;
        entity.origin = swed.ReadVec(currentPawn, Offsets.m_vOldOrigin);
        entity.view = swed.ReadVec(currentPawn, Offsets.m_vecViewOffset);
        entity.distance = Vector3.Distance(entity.origin, localPlayer.origin);
        entity.bones = reader.ReadBones(boneMatrix);
        entity.bones2d = reader.ReadBones2d(entity.bones, viewMatrix, screen);
        entity.head = swed.ReadVec(boneMatrix, 6 * 32);
        entity.head2d = Calculate.WorldToScreen(viewMatrix, entity.head,(int)renderer.overlaySize.X, (int)renderer.overlaySize.Y);
        entity.pixelDistance = Vector2.Distance(entity.head2d, new Vector2(renderer.overlaySize.X / 2, renderer.overlaySize.Y / 2));
        entities.Add(entity);
    }

    renderer.entitiesCopy = entities;
    renderer.localPlayerCopy = localPlayer;
    
    entities = entities.OrderBy(o => o.pixelDistance).ToList();
    if (entities.Count > 0 && GetAsyncKeyState(Hotkey) < 0 && renderer.aimbot)
    {
        Vector3 playerView = Vector3.Add(localPlayer.origin, localPlayer.view);
        Vector3 entityView = Vector3.Add(entities[0].origin, entities[0].view);
        if (entities[0].pixelDistance < renderer.aimbotFov)
        {
            Vector2 newAngles = Calculate.CalculateAngles(playerView, entities[0].head);
            Vector3 newAnglesVec3 = new Vector3(newAngles.Y, newAngles.X, 0.0f);
            if (renderer.silent)
            {
                swed.Nop(client + 0x529638, 3);
                swed.Nop(client + 0x52963E, 4);
            }
            swed.WriteVec(client, Offsets.dwViewAngles, newAnglesVec3);
            if (renderer.silent)
            {
                swed.WriteVec(client, Offsets.dwViewAngles, newAnglesVec3);
            }
        }
    }
    else
    {
        swed.WriteBytes(client + 0x529638, "41 89 0E");
        swed.WriteBytes(client + 0x52963E, "41 89 4E 04");
    }

    // anti recoil
    if (renderer.antiRecoil)
    {
        swed.Nop(client, 0x524058, 5);
        swed.Nop(client, 0x524067, 6);
    }
    else
    {
        swed.WriteBytes(client, 0x524058, "F3 41 0F 11 00");
        swed.WriteBytes(client, 0x524067, "F3 41 0F 11 48 04");
    }

    //triggerbot
    if (renderer.triggerBot)
    {
        if (entIndex != -1)
        {
            IntPtr currentPawn = swed.ReadPointer(listEntry, 0x78 * (entIndex & 0x1FF));
            int team = swed.ReadInt(localPlayerPawn, Offsets.m_iIDEntIndex);
            int entityTeam = swed.ReadInt(currentPawn, Offsets.m_iTeamNum);
            if (team != entityTeam)
            {
                if (GetAsyncKeyState(Hotkey) < 0 && speed <= 35)
                {
                    swed.WriteInt(client, Offsets.attack, 65537);
                    Thread.Sleep(10);
                    swed.WriteInt(client, Offsets.attack, 256);
                    Thread.Sleep(10);
                }
            }
        }
    }

    
}

[DllImport("user32.dll")]
static extern short GetAsyncKeyState(int vKey);