namespace Library.CMS.Models

public class Site
{
    
    private string owner;

    public string Owner
    {
        get
        {
            return owner;
        }

        set
        {
            if (owner != value)
            {
                owner = value;
            }
            
        }
    }
    private List<string> users;
    private List<string> content;

    public Site()
    {
        owner = string.Empty;
        users = new List<string>();
        content = new List<string>();
    }
}