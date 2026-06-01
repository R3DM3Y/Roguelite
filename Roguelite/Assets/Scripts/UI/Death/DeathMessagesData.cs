using UnityEngine;

[CreateAssetMenu(fileName = "DeathMessagesData", menuName = "Game/Death Messages Data")]
public class DeathMessagesData : ScriptableObject
{
    [Header("English (0)")]
    public string titleEN = "YOU DIED";
    public string[] messagesEN = new string[]
    {
        "The castle claims another soul...",
        "Your body crumbles, but your will remains.",
        "Death is not the end — it is a doorway.",
        "The spirit of war feasts on your failure.",
        "You fought well, but the castle fights better.",
        "Another body lost to the ancient halls.",
        "The entity watches. It always watches.",
        "Your soul slips through its grasp once more.",
        "The basement awaits its newest vessel.",
        "Try and try again — the castle demands it."
    };
    public string hintEN = "[ENTER] Find a new vessel\n[ESC] Abandon the castle";
    
    [Header("Русский (1)")]
    public string titleRU = "ВЫ ПОГИБЛИ";
    public string[] messagesRU = new string[]
    {
        "Замок забирает ещё одну душу...",
        "Твоё тело рассыпается, но воля остаётся.",
        "Смерть — это не конец, это врата.",
        "Дух войны пирует на твоих неудачах.",
        "Ты сражался храбро, но замок сражается лучше.",
        "Очередное тело потеряно в древних залах.",
        "Сущность наблюдает. Она всегда наблюдает.",
        "Твоя душа снова ускользает от её хватки.",
        "Подвал ждёт своё новое вместилище.",
        "Пытайся снова и снова — замок требует этого."
    };
    public string hintRU = "[ENTER] Найти новый сосуд\n[ESC] Покинуть замок";
    
    public string GetTitle(int languageIndex)
    {
        return languageIndex == 1 ? titleRU : titleEN;
    }
    
    public string GetRandomMessage(int languageIndex)
    {
        string[] messages = languageIndex == 1 ? messagesRU : messagesEN;
        return messages[Random.Range(0, messages.Length)];
    }
    
    public string GetHint(int languageIndex)
    {
        return languageIndex == 1 ? hintRU : hintEN;
    }
}