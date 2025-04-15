using Devdog.General.Editors;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Devdog.QuestSystemPro.Editors
{
    [CustomObjectPicker(typeof(Quest), 10)]
    public class QuestPickerEditorWindow : ObjectPickerBaseEditor
    {
        public override void Init()
        {
            base.Init();

            foundObjects = foundObjects.OrderBy(o => o.GetType().Name).ThenBy(o => ((Quest)o).ID).ToList();
        }

        protected override IEnumerable<Object> FindAssetsOfType(Type type, bool allowInherited)
        {
            return AssetDatabase.FindAssets("t:" + typeof(Quest).FullName) // Devdog.QuestSystemPro.Quest
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<Quest>(path));

            /* singleton isn't reliable in editor, with domain reload, etc.
            if (QuestManager.instance == null)
            {
                return System.Array.Empty<Object>();
            }

            return QuestManager.instance.quests;
            */
        }

        protected override IEnumerable<Object> FindAssetsWithComponent(Type type, bool allowInherited)
        {
            return System.Array.Empty<Object>();
        }

        public override bool IsSearchMatch(Object asset, string searchQuery)
        {
            searchQuery = searchQuery.ToLower();
            var q = asset as Quest;
            if (q != null)
            {
                return q.name.message.ToLower().Contains(searchQuery) ||
                       q.description.message.ToLower().Contains(searchQuery) ||
                       q.ID.ToString().Contains(searchQuery);
                //                       q.tasks.Any(o => o.key.ToLower().Contains(searchQuery) || o.description.ToLower().Contains(searchQuery))
            }

            return base.IsSearchMatch(asset, searchQuery);
        }

        protected override void DrawObject(Rect r, Object asset)
        {
            using (new GroupBlock(r, GUIContent.none, "box"))
            {
                var q = asset as Quest;
                if (q != null)
                {
                    float cellSize = r.width;

                    var labelRect = new Rect(5, 5, cellSize, EditorGUIUtility.singleLineHeight);
                    GUI.Label(labelRect, asset.GetType().Name, UnityEditor.EditorStyles.boldLabel);

                    labelRect.y += EditorGUIUtility.singleLineHeight;
                    GUI.Label(labelRect, q.name.message);

                    labelRect.y += EditorGUIUtility.singleLineHeight;
                    GUI.Label(labelRect, q.tasks.Length + " tasks");

                    return;
                }
            }

            base.DrawObject(r, asset);
        }
    }
}
