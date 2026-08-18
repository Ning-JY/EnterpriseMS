namespace EnterpriseMS.Services.AI.Models;

/// <summary>运行时 AI 配置（由 Debug 页面在线保存，持久化于 App_Data/ai-config.json）</summary>
public class AiConfigDto
{
    public string ApiKey { get; set; } = "";
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    public string Model { get; set; } = "gpt-4o";
}
