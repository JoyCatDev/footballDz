using UnityEditor;

namespace SimSoc
{
	[CustomEditor(typeof(WorldCupTournamentSettings))]
	public class WorldCupTournamentSettingsEditor : Editor
	{
		private bool _showDebug;
	
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			var t = (target as WorldCupTournamentSettings);
			if (t == null)
			{
				return;
			}
		
			EditorGUILayout.Space();
			_showDebug = EditorGUILayout.Foldout(_showDebug, "DEBUG", true);
			if (!_showDebug)
			{
				return;
			}
			EditorGUILayout.LabelField($"{nameof(t.NumTeams)}: {t.NumTeams}");
			EditorGUILayout.LabelField($"{nameof(t.NumMatches)}: {t.NumMatches}");
			EditorGUILayout.Space();
		
			EditorGUILayout.LabelField("GroupStage:", EditorStyles.boldLabel);
			EditorGUILayout.LabelField($"{nameof(t.GroupStage.NumTeamsPerGroup)}: {t.GroupStage.NumTeamsPerGroup}");
			EditorGUILayout.LabelField($"{nameof(t.GroupStage.NumGroups)}: {t.GroupStage.NumGroups}");
			EditorGUILayout.LabelField($"{nameof(t.GroupStage.NumMatchesPerGroup)}: {t.GroupStage.NumMatchesPerGroup}");
			EditorGUILayout.LabelField($"{nameof(t.GroupStage.NumMatches)}: {t.GroupStage.NumMatches}");
			EditorGUILayout.LabelField(
				$"{nameof(t.GroupStage.NumAdvanceTeamsPerGroup)}: {t.GroupStage.NumAdvanceTeamsPerGroup}");
			EditorGUILayout.LabelField($"{nameof(t.GroupStage.NumAdvanceTeams)}: {t.GroupStage.NumAdvanceTeams}");
			EditorGUILayout.Space();
		
			DisplayKnockoutSettings("RoundOf32", t.RoundOf32);
			DisplayKnockoutSettings("RoundOf16", t.RoundOf16);
			DisplayKnockoutSettings("QuarterFinals", t.QuarterFinals);
			DisplayKnockoutSettings("SemiFinals", t.SemiFinals);
			DisplayKnockoutSettings("ThirdPlace", t.ThirdPlace);
			DisplayKnockoutSettings("Final", t.Final);
		}

		private void DisplayKnockoutSettings(string heading, WorldCupKnockoutStageSettings settings)
		{
			EditorGUILayout.LabelField($"{heading}:", EditorStyles.boldLabel);
			EditorGUILayout.LabelField($"{nameof(settings.DefaultNumMatches)}: {settings.DefaultNumMatches}");
			EditorGUILayout.LabelField(
				$"{nameof(settings.TeamsNeededForAllMatches)}: {settings.TeamsNeededForAllMatches}");
			EditorGUILayout.LabelField($"{nameof(settings.NumMatches)}: {settings.NumMatches}");
			EditorGUILayout.Space();
		}
	}
}
