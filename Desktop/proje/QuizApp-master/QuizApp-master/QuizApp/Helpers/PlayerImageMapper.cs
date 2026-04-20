namespace QuizApp.Helpers;

public static class PlayerImageMapper
{
    private const string WikimediaFileRedirect = "https://commons.wikimedia.org/wiki/Special:Redirect/file/";

    public static string? MapQuestionImage(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        var value = imageUrl.Trim();

        if (value.StartsWith("/images/players/", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        if (ContainsAll(value, "cristiano", "ronaldo"))
        {
            return "/images/players/cristiano-ronaldo.jpg";
        }

        if (ContainsAll(value, "lionel", "messi"))
        {
            return "/images/players/lionel-messi.jpg";
        }

        if (ContainsAll(value, "messi"))
        {
            return "/images/players/lionel-messi.jpg";
        }

        if (ContainsAll(value, "mbappe") || ContainsAll(value, "mbapp%C3%A9") || ContainsAll(value, "mbappé"))
        {
            return WikimediaFileRedirect + "Kylian_Mbappe_2017.jpg";
        }

        if (ContainsAll(value, "neymar"))
        {
            return "/images/players/neymar.jpg";
        }

        if (ContainsAll(value, "kevin", "de", "bruyne") || ContainsAll(value, "kevin_de_bruyne"))
        {
            return "/images/players/kevin-de-bruyne.jpg";
        }

        if (ContainsAll(value, "griezmann"))
        {
            return WikimediaFileRedirect + "Antoine_Griezmann_2018.jpg";
        }

        if (ContainsAll(value, "coman"))
        {
            return WikimediaFileRedirect + "Kingsley_Coman_%282019%29_%28cropped%29.jpg";
        }

        if (ContainsAll(value, "dembele") || ContainsAll(value, "demb%C3%A9l%C3%A9") || ContainsAll(value, "dembélé"))
        {
            return WikimediaFileRedirect + "Ousmane_Demb%C3%A9l%C3%A9_2018_%28cropped%29.jpg";
        }

        if (ContainsAll(value, "aguero") || ContainsAll(value, "ag%C3%BCero") || ContainsAll(value, "agüero"))
        {
            return WikimediaFileRedirect + "Sergio_Aguero_69784.jpg";
        }

        if (ContainsAll(value, "suarez") || ContainsAll(value, "su%C3%A1rez") || ContainsAll(value, "suárez"))
        {
            return WikimediaFileRedirect + "Luis_Su%C3%A1rez_2.jpg";
        }

        if (ContainsAll(value, "di", "maria"))
        {
            return WikimediaFileRedirect + "%C3%81ngel_Di_Mar%C3%ADa_2017.jpg";
        }

        return value;
    }

    public static string? MapPlayerName(string? playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            return null;
        }

        return MapQuestionImage(playerName) switch
        {
            "/images/players/cristiano-ronaldo.jpg" => "/images/players/cristiano-ronaldo.jpg",
            "/images/players/lionel-messi.jpg" => "/images/players/lionel-messi.jpg",
            "/images/players/neymar.jpg" => "/images/players/neymar.jpg",
            "/images/players/kevin-de-bruyne.jpg" => "/images/players/kevin-de-bruyne.jpg",
            _ => null
        };
    }

    private static bool ContainsAll(string value, params string[] tokens)
    {
        return tokens.All(token => value.Contains(token, StringComparison.OrdinalIgnoreCase));
    }
}
