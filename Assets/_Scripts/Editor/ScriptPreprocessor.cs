using System;
using System.IO;
using UnityEditor;

namespace _Scripts.Editor
{
    public class ScriptPreprocessor : AssetModificationProcessor
    {
        public static void OnWillCreateAsset(string metaFilePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(metaFilePath);

            if (!fileName.EndsWith(".cs"))
                return;

            var actualFilePath = $"{Path.GetDirectoryName(metaFilePath)}{Path.DirectorySeparatorChar}{fileName}";

            var content = File.ReadAllText(actualFilePath);
            // string newContent = content.Replace("#PROJECTNAME#", PlayerSettings.productName);
            // newContent = newContent.Replace("#CREATIONDATE#",  System.DateTime.Now.Date + "");
            // newContent = newContent.Replace("#COMPANYNAME#", PlayerSettings.companyName);

            var newContent = content.Replace("#AUTHOR#", PlayerSettings.companyName);
            newContent = newContent.Replace("#CREATIONDATE#", DateTime.Now.ToString("dd-MM-yyyy"));
            newContent = newContent.Replace("#DESCRIPTION#", "A brief description of the script.");
            newContent = newContent.Replace("#LICENSE#", "Licence");

            // Generate the namespace based on the file's path
            var filePathWithoutAssets = actualFilePath.Substring(actualFilePath.IndexOf("Assets") + "Assets".Length);
            var namespacePath = filePathWithoutAssets.Replace(Path.DirectorySeparatorChar, '.').Replace(".cs", "");

            // Remove leading dot if present
            if (namespacePath.StartsWith(".")) namespacePath = namespacePath.Substring(1);

            // Ensure the namespace does not include the filename
            namespacePath = namespacePath.Substring(0, namespacePath.LastIndexOf('.'));

            newContent = newContent.Replace("#NAMESPACE#", namespacePath);

            if (content == newContent)
                return;

            File.WriteAllText(actualFilePath, newContent);
            AssetDatabase.Refresh();
        }
    }
}