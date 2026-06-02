using UnityEngine;

[CreateAssetMenu(fileName = "CreditsData", menuName = "Game/Credits Data")]
public class CreditsData : ScriptableObject
{
    [Header("English")]
    public string titleEN = "CREDITS";
    [TextArea(3, 10)]
    public string textEN = "Author\n\nSotnikov Roman\n\n\n\nMoral Support\n\nSheldon\n\nShipitsyna Polina";
    public string hintEN = "Press any key...";
    
    [Header("Русский")]
    public string titleRU = "АВТОРЫ";
    [TextArea(3, 10)]
    public string textRU = "Автор\n\nСотников Роман\n\n\n\nМоральная поддержка\n\nШелдон\n\nШипицына Полина";
    public string hintRU = "Нажмите любую клавишу...";
    
    public string GetTitle(int lang) => lang == 1 ? titleRU : titleEN;
    public string GetText(int lang) => lang == 1 ? textRU : textEN;
    public string GetHint(int lang) => lang == 1 ? hintRU : hintEN;
}