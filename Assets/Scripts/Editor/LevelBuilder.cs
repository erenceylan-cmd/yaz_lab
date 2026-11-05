using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Menü: Yazlab → Haritayı Oluştur
/// Aşağıdaki metin haritasını Ground/Walls tilemap'lerine çizer ve sahne objelerini yerleştirir.
///   #  duvar          .  zemin
///   P  oyuncu         C  companion        L  şalter
///   g  portcullis (GateDoor)              k  kale kapısı (GuardDoor)
///   t  tünel kapısı (TunnelDoor)          p  basınç plakası (PressurePlate)
///   N  bekçi (Guard)  E  çıkış alanı (EndZone)
/// Ctrl+Z ile geri alınabilir. Sonrasında Navigation → Bake yapılmalıdır.
/// </summary>
public static class LevelBuilder
{
    private const string Layout = @"
################################
#........#...........###########
#........#...........###########
#..P.....#...##......###########
#....C...g...##......###########
#........g...........#.........#
#......p.#.........N.#.....EEE.#
#........#...........k.....EEE.#
#.########...........k.....EEE.#
#.t....###...........#.....EEE.#
######.###...##......#.........#
#........#...##..##..###########
#........#.......##..###########
#...L....#...........###########
#........#...........###########
################################
";

    [MenuItem("Yazlab/Haritayı Oluştur")]
    public static void Build()
    {
        Tilemap ground = FindTilemap("Ground");
        Tilemap walls = FindTilemap("Walls");
        TileBase groundTile = FindTile("Ground");
        TileBase wallTile = FindTile("Wall");

        if (ground == null || walls == null || groundTile == null || wallTile == null)
        {
            EditorUtility.DisplayDialog("Harita oluşturulamadı",
                "Sahnede 'Ground' ve 'Walls' tilemap'leri, projede 'Ground' ve 'Wall' tile dosyaları olmalı.", "Tamam");
            return;
        }

        Undo.RegisterCompleteObjectUndo(new Object[] { ground, walls }, "Haritayı Oluştur");
        ground.ClearAllTiles();
        walls.ClearAllTiles();

        string[] rows = Layout.Replace("\r", "").Trim('\n').Split('\n');
        int height = rows.Length;
        int width = 0;
        var marks = new Dictionary<char, List<Vector3>>();

        for (int r = 0; r < height; r++)
        {
            string row = rows[r];
            width = Mathf.Max(width, row.Length);
            for (int x = 0; x < row.Length; x++)
            {
                char c = row[x];
                var cell = new Vector3Int(x, height - 1 - r, 0);

                if (c == '#') walls.SetTile(cell, wallTile);
                else if (c != ' ') ground.SetTile(cell, groundTile);

                if (c != '#' && c != '.' && c != ' ')
                {
                    if (!marks.TryGetValue(c, out var list)) marks[c] = list = new List<Vector3>();
                    list.Add(ground.GetCellCenterWorld(cell));
                }
            }
        }

        PlacePoint("Player", marks, 'P', 1.5f);
        PlacePoint("Companion", marks, 'C', 0.5f);
        PlacePoint("Lever", marks, 'L', 0.5f);
        PlacePoint("Guard", marks, 'N', 1f);
        PlaceArea("GateDoor", marks, 'g');
        PlaceArea("GuardDoor", marks, 'k');
        PlaceArea("TunnelDoor", marks, 't');
        PlacePoint("PressurePlate", marks, 'p', 0.8f);
        PlaceArea("EndZone", marks, 'E');

        Camera cam = Camera.main;
        if (cam != null)
        {
            Undo.RecordObject(cam.transform, "Kamera");
            Undo.RecordObject(cam, "Kamera");
            Vector3 center = ground.CellToWorld(new Vector3Int(0, 0, 0)) + new Vector3(width / 2f, height / 2f, 0f);
            cam.transform.position = new Vector3(center.x, center.y, -10f);
            cam.orthographicSize = Mathf.Max(height / 2f, width / 2f / (16f / 9f)) + 0.3f;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[LevelBuilder] Harita oluşturuldu. Şimdi 'Navigation' objesinde Bake yapın ve sahneyi kaydedin (Ctrl+S).");
    }

    private static void PlacePoint(string objectName, Dictionary<char, List<Vector3>> marks, char key, float scale)
    {
        GameObject go = GameObject.Find(objectName);
        if (go == null || !marks.TryGetValue(key, out var points))
        {
            Debug.LogWarning($"[LevelBuilder] '{objectName}' bulunamadı, atlandı.");
            return;
        }
        Undo.RecordObject(go.transform, "Yerleştir");
        go.transform.SetPositionAndRotation(points[0], Quaternion.identity);
        go.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private static void PlaceArea(string objectName, Dictionary<char, List<Vector3>> marks, char key)
    {
        GameObject go = GameObject.Find(objectName);
        if (go == null || !marks.TryGetValue(key, out var points))
        {
            Debug.LogWarning($"[LevelBuilder] '{objectName}' bulunamadı, atlandı.");
            return;
        }

        Vector3 min = points[0], max = points[0];
        foreach (Vector3 p in points) { min = Vector3.Min(min, p); max = Vector3.Max(max, p); }

        Undo.RecordObject(go.transform, "Yerleştir");
        go.transform.SetPositionAndRotation((min + max) / 2f, Quaternion.identity);
        go.transform.localScale = new Vector3(max.x - min.x + 1f, max.y - min.y + 1f, 1f);
    }

    private static Tilemap FindTilemap(string name)
    {
        foreach (Tilemap t in Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            if (t.name == name) return t;
        return null;
    }

    private static TileBase FindTile(string name)
    {
        foreach (string guid in AssetDatabase.FindAssets(name + " t:TileBase"))
        {
            var tile = AssetDatabase.LoadAssetAtPath<TileBase>(AssetDatabase.GUIDToAssetPath(guid));
            if (tile != null && tile.name == name) return tile;
        }
        return null;
    }
}
