namespace Server.Helpers;

public abstract record Paths
{
    private const string Main = "/api/auth";
    
    public static string Registration => $"{Main}/registration";
    public static string Login => $"{Main}/login";
    public static string Refresh => $"{Main}/refresh";
    public static string Logout => $"{Main}/logout";
    public static string EmailConfirmation => $"{Main}/email-confirmation";
    public static string ForgotPassword => $"{Main}/forgot-password";
    public static string ResetPassword => $"{Main}/reset-password";
}