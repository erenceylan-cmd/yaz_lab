/// <summary>Bir diyalog turunun sonucu: ya geçerli bir NPC yanıtı ya da oyuncuya gösterilecek hata.</summary>
public class DialogueOutcome
{
    public bool Success { get; }
    public NPCResponse Response { get; }
    public string ErrorMessage { get; }

    private DialogueOutcome(bool success, NPCResponse response, string error)
    {
        Success = success;
        Response = response;
        ErrorMessage = error;
    }

    public static DialogueOutcome Ok(NPCResponse response) => new DialogueOutcome(true, response, null);
    public static DialogueOutcome Fail(string error) => new DialogueOutcome(false, null, error);
}
