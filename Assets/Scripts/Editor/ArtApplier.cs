using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Menü: Yazlab → Görselleri Uygula
/// Assets/Art/Sprites altındaki pixel art görselleri içe aktarma ayarlarıyla (32 PPU, Point filtre)
/// hazırlar ve tile'lara / sahne objelerine atar. Renk tonlarını beyaza çeker ki görsel doğru görünsün.
/// Ctrl+Z ile sahne değişiklikleri geri alınabilir.
/// </summary>
public static class ArtApplier
{
    private const string Folder = "Assets/Art/Sprites/";

    [MenuItem("Yazlab/Görselleri Uygula")]
    public static void Apply()
    {
        string[] names = { "grass", "wall", "player", "companion", "guard", "door", "plate", "lever" };
        foreach (string n in names) ConfigureImporter(Folder + n + ".png");
        AssetDatabase.Refresh();

        // Tile'lar
        SetTile("Ground", "grass");
        SetTile("Wall", "wall");
        foreach (Tilemap map in Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
        {
            Undo.RecordObject(map, "Tilemap rengi");
            map.color = Color.white;
            map.RefreshAllTiles();
        }

        // Sahne objeleri
        SetSprite("Player", "player");
        SetSprite("Companion", "companion");
        SetSprite("Guard", "guard");
        SetSprite("Lever", "lever");
        SetSprite("PressurePlate", "plate");
        foreach (string door in new[] { "GateDoor", "TunnelDoor", "GuardDoor", "PuzzleDoor" })
            SetSprite(door, "door");
        for (int i = 1; i <= 9; i++)
            SetSprite("Plate" + i, "plate", keepColor: true);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("[ArtApplier] Görseller uygulandı. Navigation → Bake yapıp sahneyi kaydedin (Ctrl+S).");
    }

    private static void ConfigureImporter(string path)
    {
        if (AssetImporter.GetAtPath(path) is not TextureImporter ti)
        {
            Debug.LogWarning("[ArtApplier] Bulunamadı: " + path);
            return;
        }
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = 32;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.mipmapEnabled = false;
        ti.SaveAndReimport();
    }

    private static Sprite LoadSprite(string name) =>
        AssetDatabase.LoadAssetAtPath<Sprite>(Folder + name + ".png");

    private static void SetTile(string tileName, string spriteName)
    {
        Sprite sprite = LoadSprite(spriteName);
        foreach (string guid in AssetDatabase.FindAssets(tileName + " t:Tile"))
        {
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(AssetDatabase.GUIDToAssetPath(guid));
            if (tile == null || tile.name != tileName) continue;
            Undo.RecordObject(tile, "Tile görseli");
            tile.sprite = sprite;
            tile.color = Color.white;
            EditorUtility.SetDirty(tile);
        }
    }

    private static void SetSprite(string objectName, string spriteName, bool keepColor = false)
    {
        GameObject go = GameObject.Find(objectName);
        if (go == null || !go.TryGetComponent(out SpriteRenderer sr))
        {
            Debug.LogWarning($"[ArtApplier] '{objectName}' bulunamadı, atlandı.");
            return;
        }
        Undo.RecordObject(sr, "Sprite");
        sr.sprite = LoadSprite(spriteName);
        if (!keepColor) sr.color = Color.white;
    }
}
