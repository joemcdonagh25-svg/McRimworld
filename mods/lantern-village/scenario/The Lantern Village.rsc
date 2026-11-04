<?xml version="1.0" encoding="utf-8"?>
<savedscenario>
	<meta>
		<gameVersion>1.6.4871 rev590</gameVersion>
		<modIds>
			<li>ludeon.rimworld</li>
			<li>ludeon.rimworld.ideology</li>
			<li>ludeon.rimworld.biotech</li>
		</modIds>
		<modSteamIds>
			<li>0</li>
			<li>1392840</li>
			<li>1826140</li>
		</modSteamIds>
		<modNames>
			<li>Core</li>
			<li>Ideology</li>
			<li>Biotech</li>
		</modNames>
	</meta>
	<scenario>
		<name>The Lantern Village</name>
		<summary>Five villagers settle into a prebuilt boreal woodland hamlet. Inside the light is community. Outside the light is the unknown.</summary>
		<description>Grimm-inspired folk horror. An elder, a woodsman, a hunter, a smith and a healer arrive on foot at a partly-palisaded medieval hamlet in a boreal forest - great hall, cottages, kitchen, smithy, barn and gardens already raised. Over roughly two years, ordinary forest dangers give way to folklore threats, then to something stranger. Keep the lanterns burning and finish the walls before the forest finishes you.</description>
		<playerFaction>
			<def>PlayerFaction</def>
			<factionDef>PlayerColony</factionDef>
		</playerFaction>
		<surfaceLayer>
			<def>SurfaceLayerFixed</def>
			<layer>Surface</layer>
			<settingsDef>Surface</settingsDef>
			<hide>True</hide>
			<tag>Surface</tag>
			<connections>
				<li>
					<tag>Orbit</tag>
					<zoomMode>ZoomOut</zoomMode>
				</li>
			</connections>
		</surfaceLayer>
		<parts>
			<li Class="ScenPart_PlanetLayer">
				<def>PlanetLayer</def>
				<layer>Orbit</layer>
				<settingsDef>Orbit</settingsDef>
				<hide>True</hide>
				<tag>Orbit</tag>
				<connections>
					<li>
						<tag>Surface</tag>
						<zoomMode>ZoomIn</zoomMode>
					</li>
				</connections>
			</li>
			<li Class="ScenPart_ConfigPage_ConfigureStartingPawns">
				<def>ConfigPage_ConfigureStartingPawns</def>
				<pawnChoiceCount>5</pawnChoiceCount>
				<pawnCount>5</pawnCount>
			</li>
			<!-- Hamlet before pawn arrival / starting gear so walls exist first and we don't wipe supplies (Vampire Lord keep pattern). -->
			<li Class="LanternVillage.ScenPart_LanternVillageSetup">
				<def>LanternVillageSetup</def>
			</li>
			<li Class="ScenPart_PlayerPawnsArriveMethod">
				<def>PlayerPawnsArriveMethod</def>
				<method>Standing</method>
			</li>
			<li Class="ScenPart_GameStartDialog">
				<def>GameStartDialog</def>
				<text>Five settlers step into a woodland hamlet with the last of the daylight.

The great hall hearth is lit. Cottages, kitchen, smithy and barn stand ready. The palisade is unfinished — enough to feel safe, not enough to be safe.

Inside the light is community. Outside the light is the unknown.

Keep your lanterns burning. The forest will test you slowly at first -- and then less slowly.</text>
				<textKey>GameStartDialog</textKey>
				<closeSound>GameStartSting</closeSound>
			</li>
			<li Class="ScenPart_StartingThing_Defined">
				<def>StartingThing_Defined</def>
				<thingDef>Pemmican</thingDef>
				<count>600</count>
			</li>
			<li Class="ScenPart_StartingThing_Defined">
				<def>StartingThing_Defined</def>
				<thingDef>MedicineHerbal</thingDef>
				<count>20</count>
			</li>
			<li Class="ScenPart_StartingThing_Defined">
				<def>StartingThing_Defined</def>
				<thingDef>WoodLog</thingDef>
				<count>500</count>
			</li>
			<li Class="ScenPart_StartingThing_Defined">
				<def>StartingThing_Defined</def>
				<thingDef>Bow_Short</thingDef>
				<count>2</count>
			</li>
			<li Class="ScenPart_StartingThing_Defined">
				<def>StartingThing_Defined</def>
				<thingDef>Bow_Recurve</thingDef>
				<count>1</count>
			</li>
			<li Class="ScenPart_StartingThing_Defined">
				<def>StartingThing_Defined</def>
				<thingDef>MeleeWeapon_Knife</thingDef>
				<stuff>Steel</stuff>
				<count>1</count>
			</li>
			<li Class="ScenPart_StartingThing_Defined">
				<def>StartingThing_Defined</def>
				<thingDef>MeleeWeapon_Club</thingDef>
				<stuff>WoodLog</stuff>
				<count>1</count>
			</li>
			<li Class="ScenPart_StartingThing_Defined">
				<def>StartingThing_Defined</def>
				<thingDef>Silver</thingDef>
				<count>150</count>
			</li>
			<li Class="ScenPart_StartingAnimal">
				<def>StartingAnimal</def>
				<animalKind>Muffalo</animalKind>
				<count>2</count>
			</li>
		</parts>
	</scenario>
</savedscenario>
