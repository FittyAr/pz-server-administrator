import flet as ft
import os
from typing import Optional, Callable

class AdvancedTextEditorControl:
    """Control para edición avanzada de archivos de configuración mediante texto plano"""
    
    def __init__(self, on_save: Optional[Callable[[str], None]] = None):
        self.on_save = on_save
        self.current_content = ""
        self.file_path = None
        self.file_type = None
        
        # Editor de texto principal
        self.text_editor = ft.TextField(
            multiline=True,
            min_lines=20,
            max_lines=30,
            expand=True,
            border_color=ft.Colors.OUTLINE,
            focused_border_color=ft.Colors.PRIMARY,
            text_style=ft.TextStyle(font_family="Consolas"),
            value="# Selecciona un archivo de configuración para editarlo"
        )
        
        # Información del archivo
        self.file_info_text = ft.Text(
            "Archivo: No seleccionado",
            size=12,
            color=ft.Colors.ON_SURFACE_VARIANT
        )
        
        # Botones de acción
        self.save_button = ft.ElevatedButton(
            text="Guardar",
            icon=ft.Icons.SAVE,
            on_click=self._on_save_click,
            disabled=True
        )
        
        self.reset_button = ft.OutlinedButton(
            text="Restaurar",
            icon=ft.Icons.REFRESH,
            on_click=self._on_reset_click,
            disabled=True
        )
        
        # Contenedor principal
        self.container = ft.Container(
            content=ft.Column([
                # Información del archivo
                ft.Container(
                    content=self.file_info_text,
                    padding=ft.padding.only(bottom=10)
                ),
                
                # Editor de texto
                ft.Container(
                    content=self.text_editor,
                    expand=True,
                    border_radius=8
                ),
                
                # Botones de acción
                ft.Container(
                    content=ft.Row([
                        self.save_button,
                        self.reset_button
                    ], spacing=10),
                    padding=ft.padding.only(top=10)
                )
            ]),
            padding=10,
            expand=True
        )
    
    def load_file_content(self, file_path: str, file_type: str):
        """Cargar contenido de un archivo para edición"""
        self.file_path = file_path
        self.file_type = file_type
        
        if file_path and os.path.exists(file_path):
            try:
                # Intentar diferentes codificaciones
                encodings = ['utf-8', 'latin-1', 'cp1252', 'iso-8859-1']
                content = None
                
                for encoding in encodings:
                    try:
                        with open(file_path, 'r', encoding=encoding) as file:
                            content = file.read()
                            break
                    except UnicodeDecodeError:
                        continue
                
                if content is not None:
                    self.current_content = content
                    self.text_editor.value = content
                    self.file_info_text.value = f"Archivo: {os.path.basename(file_path)}"
                    self._enable_buttons()
                else:
                    self._show_error(f"Error: No se pudo decodificar el archivo con ninguna codificación")
                    
            except Exception as e:
                self._show_error(f"Error al cargar el archivo: {str(e)}")
        else:
            # Archivo no existe, mostrar plantilla
            template = self._get_template_for_type(file_type)
            self.current_content = template
            self.text_editor.value = template
            self.file_info_text.value = f"Archivo: {os.path.basename(file_path) if file_path else 'Nuevo archivo'} (No existe - Plantilla)"
            self._enable_buttons()
    
    def _get_template_for_type(self, file_type: str) -> str:
        """Obtener plantilla según el tipo de archivo"""
        templates = {
            "server_settings": """# Configuración del Servidor Project Zomboid
# Archivo de configuración principal del servidor

PublicName=My PZ Server
PublicDescription=A Project Zomboid Server
MaxPlayers=32
PVP=true
PauseEmpty=true
PingLimit=400
HoursForLootRespawn=0
SaveWorldEveryMinutes=0
PlayerSafehouse=false
AdminSafehouse=false
SafehouseAllowTrepass=true
SafehouseAllowFire=true
SafehouseAllowLoot=true
SafehouseAllowRespawn=false
SafehouseDaySurvivedToClaim=0
SafeHouseRemovalTime=144
AllowDestructionBySledgehammer=true
KickFastPlayers=false
ServerPlayerID=27015
Port=16261
BindIP=0.0.0.0
RCONPort=27015
RCONPassword=
Password=
MaxAccountsPerUser=0""",
            "sandbox_vars": """-- Variables de Sandbox para Project Zomboid
-- Configuración del mundo y reglas del juego

SandBoxVars = {
    Version = 5,
    -- Configuración de Zombies
    Zombies = 4, -- 1=Extremely Rare, 2=Rare, 3=Sometimes, 4=Often, 5=Very Often
    Distribution = 1, -- 1=Urban Focused
    DayLength = 3, -- 1=15 min, 2=30 min, 3=1 hour, 4=2 hours, 5=3 hours, 6=4 hours, 7=5 hours, 8=6 hours, 9=7 hours, 10=8 hours, 11=9 hours, 12=10 hours, 13=11 hours, 14=12 hours
    StartMonth = 7, -- 1=January, 7=July
    StartTime = 2, -- 1=9 AM, 2=12 PM, 3=6 PM, 4=2 AM
    WaterShut = 6, -- Días hasta que se corte el agua (0-30)
    ElecShut = 6, -- Días hasta que se corte la electricidad (0-30)
    WaterShutModifier = 14,
    ElecShutModifier = 14,
    FoodLoot = 2,
    CannedFoodLoot = 2,
    LiteratureLoot = 2,
    SurvivalGearsLoot = 2,
    MedicalLoot = 2,
    WeaponLoot = 2,
    RangedWeaponLoot = 2,
    AmmoLoot = 2,
    MechanicsLoot = 2,
    OtherLoot = 2,
    Temperature = 3, -- 1=Very Cold, 2=Cold, 3=Normal, 4=Hot
    Rain = 3, -- 1=Very Dry, 2=Dry, 3=Normal, 4=Rainy
    ErosionSpeed = 3, -- 1=Very Fast (20 days), 2=Fast (50 days), 3=Normal (100 days), 4=Slow (200 days), 5=Very Slow (500 days)
    ErosionDays = 0,
    XpMultiplier = 1.0,
    ZombieAttractionMultiplier = 1.0,
    VehicleEasyUse = false,
    Farming = 3, -- 1=Very Fast, 2=Fast, 3=Normal, 4=Slow
    CompostTime = 2, -- 1=1 Week, 2=2 Week, 3=3 Week, 4=4 Week, 5=6 Week, 6=8 Week, 7=10 Week
    StatsDecrease = 3, -- 1=Very Fast, 2=Fast, 3=Normal, 4=Slow
    NatureAbundance = 3, -- 1=Very Poor, 2=Poor, 3=Normal, 4=Abundant
    Alarm = 4, -- 1=Never, 2=Extremely Rare, 3=Rare, 4=Sometimes, 5=Often
    LockedHouses = 6, -- 1=Never, 2=Extremely Rare, 3=Rare, 4=Sometimes, 5=Often, 6=Very Often
    FoodRotSpeed = 3, -- 1=Very Fast, 2=Fast, 3=Normal, 4=Slow
    FridgeFactor = 3, -- 1=Very Low, 2=Low, 3=Normal, 4=High
    LootRespawn = 1, -- 1=None, 2=Every Day, 3=Every Week, 4=Every Month
    LootSeenPreventRespawn = true,
    ThumpOnConstruction = true,
    ThumpNoChasing = false,
    PVP = true,
    PVPMeleeWhileHitReaction = false,
    PVPMeleeDamageModifier = 30.0,
    PVPFirearmDamageModifier = 50.0,
    CarDamageOnImpact = 3, -- 1=Very Low, 2=Low, 3=Normal, 4=High
    DamageToPlayerFromHitByACar = 1, -- 1=None, 2=Low, 3=Normal, 4=High
    TrafficJam = true,
    CarGeneralCondition = 2, -- 1=Very Low, 2=Low, 3=Normal, 4=High
    CarHasGas = 2, -- 1=Very Low, 2=Low, 3=Normal, 4=High, 5=Full
    ChanceHasGas = 70,
    CarSpawnRate = 3, -- 1=None, 2=Very Low, 3=Low, 4=Normal, 5=High
    CarAlarm = 2, -- 1=Never, 2=Extremely Rare, 3=Rare, 4=Sometimes, 5=Often, 6=Very Often
    PlayerDamageFromCrash = true,
    SirenShutoffHours = 0.0,
    RearVulnerability = 3, -- 1=Low, 2=Medium, 3=High
    AttackBlockMovements = true,
    AllClothesUnlocked = false,
}""",
            "spawn_regions": """-- Regiones de Spawn para Project Zomboid
-- Define las áreas donde los jugadores pueden aparecer

function SpawnRegions()
    return {
        { name = "Muldraugh, KY", file = "media/maps/Muldraugh, KY/spawnpoints.lua" },
        { name = "West Point, KY", file = "media/maps/West Point, KY/spawnpoints.lua" },
        { name = "Riverside, KY", file = "media/maps/Riverside, KY/spawnpoints.lua" },
        { name = "Rosewood, KY", file = "media/maps/Rosewood, KY/spawnpoints.lua" },
        { name = "March Ridge, KY", file = "media/maps/March Ridge, KY/spawnpoints.lua" },
        { name = "Dixie, KY", file = "media/maps/Dixie, KY/spawnpoints.lua" },
        { name = "Valley Station, KY", file = "media/maps/Valley Station, KY/spawnpoints.lua" },
        { name = "Brandenburg, KY", file = "media/maps/Brandenburg, KY/spawnpoints.lua" },
    }
end""",
            "server_rules": """{
  "_comment": "Reglas del servidor Project Zomboid",
  "version": "1.0",
  "rules": [
    "No griefing - No destruir las construcciones de otros jugadores sin motivo",
    "No cheating - No usar hacks, exploits o modificaciones no autorizadas",
    "Respeto - Tratar a todos los jugadores con respeto",
    "No spam - No enviar mensajes repetitivos o innecesarios",
    "No lenguaje ofensivo - Mantener un ambiente amigable",
    "No kill on sight (KOS) - PvP solo en zonas designadas o con motivo RP",
    "Roleplay encouraged - Se fomenta el juego de rol"
  ],
  "punishments": {
    "warning": "Primera ofensa - Advertencia",
    "kick": "Segunda ofensa - Expulsión temporal",
    "temp_ban": "Tercera ofensa - Baneo temporal (1-7 días)",
    "permanent_ban": "Ofensas graves o reincidencia - Baneo permanente"
  },
  "contact": {
    "admin": "admin@servidor.com",
    "discord": "https://discord.gg/servidor",
    "website": "https://servidor.com"
  }
}"""
        }
        
        return templates.get(file_type, f"# Archivo de configuración: {file_type}\n# Contenido por defecto")
    
    def _show_error(self, message: str):
        """Mostrar mensaje de error en el editor"""
        self.text_editor.value = f"# ERROR: {message}\n# Ruta: {self.file_path}"
        self.file_info_text.value = "Error al cargar archivo"
        self._disable_buttons()
    
    def _enable_buttons(self):
        """Habilitar botones de acción"""
        self.save_button.disabled = False
        self.reset_button.disabled = False
    
    def _disable_buttons(self):
        """Deshabilitar botones de acción"""
        self.save_button.disabled = True
        self.reset_button.disabled = True
    
    def _on_save_click(self, e):
        """Manejar clic en botón guardar"""
        if self.on_save:
            content = self.text_editor.value
            self.on_save(content)
    
    def _on_reset_click(self, e):
        """Manejar clic en botón restaurar"""
        if self.file_path:
            self.load_file_content(self.file_path, self.file_type)
            if hasattr(e, 'page') and e.page:
                e.page.show_snack_bar(
                    ft.SnackBar(
                        content=ft.Text("Contenido restaurado"),
                        bgcolor=ft.Colors.ORANGE
                    )
                )
                e.page.update()
    
    def get_content(self) -> str:
        """Obtener el contenido actual del editor"""
        return self.text_editor.value
    
    def set_content(self, content: str):
        """Establecer el contenido del editor"""
        self.text_editor.value = content
        self.current_content = content
    
    def get_control(self):
        """Obtener el control principal"""
        return self.container
    
    def clear(self):
        """Limpiar el editor"""
        self.text_editor.value = "# Selecciona un archivo de configuración para editarlo"
        self.file_info_text.value = "Archivo: No seleccionado"
        self.file_path = None
        self.file_type = None
        self.current_content = ""
        self._disable_buttons()