using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Companion tekrarlanabilirlik testi (rapordaki "başarı oranı / ortalama süre" için).
/// Play modunda F9: companion'ı rastgele (ama seed'li, yani tekrarlanabilir) başlangıç noktalarına koyar,
/// şalteri sıfırlar, görevi verir ve süreyi ölçer.
/// Sonuçlar Console'a ve proje kökündeki TestResults/ klasörüne (CSV + Markdown tablo) yazılır.
/// </summary>
public class CompanionTestRunner : MonoBehaviour
{
    [SerializeField] private CompanionAgent companion;
    [SerializeField] private Lever lever;
    [SerializeField] private Door gateDoor;
    [Tooltip("Test sırasında açık tutulan tünel kapısı (basınç plakası yerine).")]
    [SerializeField] private Door tunnelDoor;

    [Header("Test ayarları")]
    [SerializeField] private int trialCount = 20;
    [SerializeField] private float trialTimeout = 30f;
    [SerializeField] private int randomSeed = 42;
    [Tooltip("Başlangıç noktalarının seçileceği dünya alanları (x, y, genişlik, yükseklik).")]
    [SerializeField] private Rect[] spawnAreas = { new Rect(1f, 8f, 8f, 7f) };
    [Tooltip("Açıksa şalter de her denemede mekanizma odasında rastgele bir yere taşınır.")]
    [SerializeField] private bool randomizeLever = true;
    [SerializeField] private Rect leverArea = new Rect(1.5f, 1.5f, 7f, 3f);
    [SerializeField] private KeyCode runKey = KeyCode.F9;

    private struct TrialResult
    {
        public int Index;
        public Vector2 Start;
        public Vector2 LeverPos;
        public bool Success;
        public float Seconds;
        public float PathLength;
        public string Note;
    }

    private bool _running;
    private string _status = "";

    private void Awake()
    {
        if (companion == null) companion = FindFirstObjectByType<CompanionAgent>();
        if (lever == null) lever = FindFirstObjectByType<Lever>();
        if (gateDoor == null) gateDoor = GameObject.Find("GateDoor")?.GetComponent<Door>();
        if (tunnelDoor == null) tunnelDoor = GameObject.Find("TunnelDoor")?.GetComponent<Door>();
    }

    private void Update()
    {
        if (!_running && Input.GetKeyDown(runKey))
            StartCoroutine(RunAll());
    }

    private IEnumerator RunAll()
    {
        if (companion == null || lever == null)
        {
            Debug.LogError("[Test] Companion veya Lever bulunamadı.");
            yield break;
        }

        _running = true;
        var rng = new System.Random(randomSeed);
        var results = new List<TrialResult>();
        Vector3 originalLeverPos = lever.transform.position;

        for (int i = 1; i <= trialCount; i++)
        {
            _status = $"Companion testi {i}/{trialCount}...";

            lever.ResetLever();
            if (gateDoor != null) gateDoor.Close();
            if (tunnelDoor != null) tunnelDoor.Open(); // oyuncunun plakada durmasını simüle eder

            if (randomizeLever)
                lever.transform.position = RandomNavMeshPoint(rng, new[] { leverArea });

            Vector3 start = RandomNavMeshPoint(rng, spawnAreas);
            companion.Agent.Warp(start);
            yield return null;

            var result = new TrialResult
            {
                Index = i,
                Start = start,
                LeverPos = lever.transform.position,
                PathLength = PathLength(start, lever.InteractionPoint.position, companion.Agent.areaMask)
            };

            float t0 = Time.time;
            if (!companion.TryCommandInteract(lever))
            {
                result.Note = "Yol bulunamadı";
                results.Add(result);
                continue;
            }

            while (lever.CanInteract && Time.time - t0 < trialTimeout)
            {
                var state = companion.StateMachine.Current;
                if (state != companion.GoToTarget && state != companion.Interact && state != companion.WaitForPath)
                    break; // görev iptal edildi
                yield return null;
            }

            result.Success = !lever.CanInteract;
            result.Seconds = Time.time - t0;
            result.Note = result.Success ? "OK" : (Time.time - t0 >= trialTimeout ? "Zaman aşımı" : "Görev iptal");
            results.Add(result);

            yield return new WaitForSeconds(0.3f);
        }

        lever.transform.position = originalLeverPos;
        lever.ResetLever();
        if (gateDoor != null) gateDoor.Close();
        if (tunnelDoor != null) tunnelDoor.Close();

        WriteReport(results);
        _status = "";
        _running = false;
    }

    private Vector3 RandomNavMeshPoint(System.Random rng, Rect[] areas)
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            Rect area = areas[rng.Next(areas.Length)];
            var p = new Vector3(area.x + (float)rng.NextDouble() * area.width,
                                area.y + (float)rng.NextDouble() * area.height, 0f);
            if (NavMesh.SamplePosition(p, out NavMeshHit hit, 0.5f, NavMesh.AllAreas))
                return hit.position;
        }
        return companion.transform.position;
    }

    private static float PathLength(Vector3 from, Vector3 to, int areaMask)
    {
        var path = new NavMeshPath();
        if (!NavMesh.CalculatePath(from, to, areaMask, path) || path.corners.Length < 2) return -1f;
        float length = 0f;
        for (int i = 1; i < path.corners.Length; i++) length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        return length;
    }

    private void WriteReport(List<TrialResult> results)
    {
        var ci = CultureInfo.InvariantCulture;
        int success = 0;
        float totalTime = 0f;
        foreach (var r in results) if (r.Success) { success++; totalTime += r.Seconds; }
        float rate = 100f * success / Mathf.Max(1, results.Count);
        float avg = success > 0 ? totalTime / success : 0f;

        var csv = new StringBuilder("deneme,baslangic_x,baslangic_y,salter_x,salter_y,yol_uzunlugu,basarili,sure_sn,not\n");
        var md = new StringBuilder();
        md.AppendLine($"**Deneme:** {results.Count} | **Başarı oranı:** %{rate.ToString("0.0", ci)} | **Ortalama süre:** {avg.ToString("0.00", ci)} sn | **Seed:** {randomSeed}");
        md.AppendLine();
        md.AppendLine("| # | Companion başlangıcı (x, y) | Şalter (x, y) | Yol uzunluğu | Sonuç | Süre (sn) |");
        md.AppendLine("|---|---|---|---|---|---|");

        foreach (var r in results)
        {
            string x = r.Start.x.ToString("0.00", ci), y = r.Start.y.ToString("0.00", ci);
            string lx = r.LeverPos.x.ToString("0.00", ci), ly = r.LeverPos.y.ToString("0.00", ci);
            string len = r.PathLength.ToString("0.00", ci), sec = r.Seconds.ToString("0.00", ci);
            csv.AppendLine($"{r.Index},{x},{y},{lx},{ly},{len},{(r.Success ? 1 : 0)},{sec},{r.Note}");
            md.AppendLine($"| {r.Index} | ({x}, {y}) | ({lx}, {ly}) | {len} | {(r.Success ? "✅" : "❌ " + r.Note)} | {sec} |");
        }

        string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "TestResults"));
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "companion_tests.csv"), csv.ToString(), Encoding.UTF8);
        File.WriteAllText(Path.Combine(dir, "companion_tests.md"), md.ToString(), Encoding.UTF8);

        Debug.Log($"[Test] Bitti. Başarı: {success}/{results.Count} (%{rate:0.0}), ortalama süre: {avg:0.00} sn\n" +
                  $"Sonuçlar: {dir}\n\n{md}");
    }

    private void OnGUI()
    {
        if (string.IsNullOrEmpty(_status)) return;
        GUI.Box(new Rect(Screen.width - 260, 8, 250, 26), _status);
    }
}
