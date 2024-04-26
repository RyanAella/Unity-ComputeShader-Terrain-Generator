using System.IO;
using UnityEditor;

namespace _Scripts.Editor
{
    public class ScriptPreprocessor : UnityEditor.AssetModificationProcessor
    {

        public static void OnWillCreateAsset(string metaFilePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(metaFilePath);

            if (!fileName.EndsWith(".cs"))
                return;

            string actualFilePath = $"{Path.GetDirectoryName(metaFilePath)}{Path.DirectorySeparatorChar}{fileName}";

            string content = File.ReadAllText(actualFilePath);
            // string newContent = content.Replace("#PROJECTNAME#", PlayerSettings.productName);
            // newContent = newContent.Replace("#CREATIONDATE#",  System.DateTime.Now.Date + "");
            // newContent = newContent.Replace("#COMPANYNAME#", PlayerSettings.companyName);
            
            string newContent = content.Replace("#AUTHOR#", PlayerSettings.companyName);
            newContent = newContent.Replace("#CREATIONDATE#", System.DateTime.Now.ToString("dd-MM-yyyy"));
            newContent = newContent.Replace("#DESCRIPTION#", "A brief description of the script.");
            newContent = newContent.Replace("#LICENSE#", "Licence");
        
            // Generate the namespace based on the file's path
            string filePathWithoutAssets = actualFilePath.Substring(actualFilePath.IndexOf("Assets") + "Assets".Length);
            string namespacePath = filePathWithoutAssets.Replace(Path.DirectorySeparatorChar, '.').Replace(".cs", "");

            // Remove leading dot if present
            if (namespacePath.StartsWith("."))
            {
                namespacePath = namespacePath.Substring(1);
            }

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