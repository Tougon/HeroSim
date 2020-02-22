using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DialogueUtilities
{
    /// <summary>
    /// Replaces the placeholder text with the Entity's name, taking into account punctuation and article use.
    /// </summary>
    public static string ReplacePlaceholderWithEntityName(string sequence, EntityParams entity, string placeholder)
    {
        if (entity.useArticle)
        {
            string name = entity.article + " " + entity.entityName.ToLower();

            // Replace all instances that are at the beginning of a sentence
            sequence = Regex.Replace(sequence, @"(^|\. |\? |\! )\" + placeholder, "$1" + name);
            sequence = sequence.Replace(placeholder, name.ToLower());

        }
        else
            sequence = sequence.Replace(placeholder, entity.entityName);

        return sequence;
    }


    /// <summary>
    /// Stock replacement function that replaces all instances of the given placeholder with the given value.
    /// </summary>
    public static string ReplacePlaceholderWithText(string sequence, string val, string placeholder)
    {
        return sequence.Replace(placeholder, val);
    }
}
