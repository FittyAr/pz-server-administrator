import flet as ft
import os
from typing import Optional, Callable
from utils.config_loader import config_loader
from controls.edit_mode_control import EditModeControl
from controls.ini_simple_editor_control import IniSimpleEditorControl
from controls.lua_simple_editor_control import LuaSimpleEditorControl
from controls.advanced_text_editor_control import AdvancedTextEditorControl
from controls.config_file_buttons_control import ConfigFileButtonsControl

class ConfigManagerControl:
    """Control principal para la gestión de archivos de configuración del servidor"""
    
    def __init__(self):
        self.current_server_id = None
        self.selected_file_type = "ini"  # Tipo de archivo seleccionado
        self.current_mode = "simple"  # Modo de edición actual
        
        # Información del servidor
        self.server_info_text = ft.Text(
            "Servidor: No seleccionado",
            size=14,
            color=ft.Colors.ON_SURFACE_VARIANT
        )
        
        # Controles especializados
        self.edit_mode_control = EditModeControl(on_mode_change=self._on_mode_change)
        self.file_buttons_control = ConfigFileButtonsControl(on_config_file_click=self._on_file_change)
        self.ini_simple_editor = IniSimpleEditorControl()
        self.lua_simple_editor = LuaSimpleEditorControl()
        self.advanced_text_editor = AdvancedTextEditorControl(on_save=self._on_advanced_save)
        
        # Contenedor para el editor actual
        self.editor_container = ft.Container(expand=True)
        
        # Botones de acción
        self.save_button = ft.ElevatedButton(
            text="Guardar Cambios",
            icon=ft.Icons.SAVE,
            on_click=self._on_save_click,
            bgcolor=ft.Colors.PRIMARY,
            color=ft.Colors.ON_PRIMARY
        )
        
        self.reset_button = ft.OutlinedButton(
            text="Restaurar",
            icon=ft.Icons.REFRESH,
            on_click=self._on_reset_click
        )
        
        self.validate_button = ft.OutlinedButton(
            text="Validar Sintaxis",
            icon=ft.Icons.CHECK_CIRCLE,
            on_click=self._on_validate_click
        )
        
        # Inicializar la interfaz
        self._update_server_info()
        self._update_editor_view()
    
    def _on_mode_change(self, mode: str):
        """Callback cuando cambia el modo de edición"""
        self.current_mode = mode
        self._update_editor_view()
        
        # Forzar actualización de la página
        self._force_update()
    
    def _on_file_change(self, file_type: str):
        """Callback cuando cambia el tipo de archivo seleccionado"""
        self.selected_file_type = file_type
        self._update_editor_view()
    
    def _update_server_info(self):
        """Actualizar información del servidor seleccionado"""
        self.current_server_id = config_loader.get_selected_server()
        if self.current_server_id:
            servers = config_loader.get_all_servers()
            server_data = servers.get(self.current_server_id, {})
            server_name = server_data.get('name', self.current_server_id)
            self.server_info_text.value = f"Servidor: {server_name}"
        else:
            self.server_info_text.value = "Servidor: No seleccionado"
    
    def _update_editor_view(self):
        """Actualizar la vista del editor según el modo y tipo de archivo"""
        if not self.current_server_id:
            self.editor_container.content = ft.Container(
                content=ft.Text(
                    "No hay servidor seleccionado\nSelecciona un servidor en la sección 'Selector de Servidores'",
                    text_align=ft.TextAlign.CENTER,
                    size=16,
                    color=ft.Colors.ON_SURFACE_VARIANT
                ),
                alignment=ft.alignment.center,
                height=300
            )
            self._force_update()
            return
        
        # Determinar qué editor usar
        if self.current_mode == "simple" and self.selected_file_type == "ini":
            # Modo simple para archivos INI
            self.editor_container.content = self.ini_simple_editor.get_control()
        elif self.current_mode == "simple" and self.selected_file_type == "lua":
            # Modo simple para archivos Lua (SandBoxVars)
            self.editor_container.content = self.lua_simple_editor.get_control()
        else:
            # Modo avanzado o archivos no soportados en modo simple
            self.editor_container.content = self.advanced_text_editor.get_control()
        
        # Cargar contenido del archivo
        self._load_file_content()
        
        # Forzar actualización de la interfaz
        self._force_update()
    
    def _load_file_content(self):
        """Cargar contenido del archivo seleccionado"""
        if not self.current_server_id:
            return
        
        file_path = self._get_config_file_path()
        
        if self.current_mode == "simple" and self.selected_file_type == "ini":
            # Cargar en editor simple INI
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
                        self.ini_simple_editor.load_ini_content(content)
                    else:
                        self.ini_simple_editor.clear()
                        print(f"Error cargando archivo INI: No se pudo decodificar con ninguna codificación")
                except Exception as e:
                    self.ini_simple_editor.clear()
                    print(f"Error cargando archivo INI: {e}")
            else:
                # Cargar plantilla por defecto
                template = self._get_template_content()
                self.ini_simple_editor.load_ini_content(template)
        elif self.current_mode == "simple" and self.selected_file_type == "lua":
            # Cargar en editor simple Lua
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
                        self.lua_simple_editor.load_lua_content(content)
                    else:
                        self.lua_simple_editor.clear()
                        print(f"Error cargando archivo Lua: No se pudo decodificar con ninguna codificación")
                except Exception as e:
                    self.lua_simple_editor.clear()
                    print(f"Error cargando archivo Lua: {e}")
            else:
                # Cargar plantilla por defecto
                template = self._get_template_content()
                self.lua_simple_editor.load_lua_content(template)
        else:
            # Cargar en editor avanzado
            internal_type = self._get_internal_file_type()
            self.advanced_text_editor.load_file_content(file_path, internal_type)
    
    def _get_config_file_path(self) -> Optional[str]:
        """Obtener la ruta del archivo de configuración actual"""
        if not self.current_server_id:
            return None
        
        servers = config_loader.get_all_servers()
        server_data = servers.get(self.current_server_id, {})
        server_path = server_data.get('server_path')
        server_name = server_data.get('name')
        
        if not server_path or not server_name:
            return None
        
        # Mapeo de tipos de archivo a nombres de archivo
        file_mappings = {
            "ini": f"{server_name}.ini",
            "lua": f"{server_name}_SandBoxVars.lua",
            "spawn_regions": f"{server_name}_spawnregions.lua",
            "spawn_points": f"{server_name}_rules.json"
        }
        
        filename = file_mappings.get(self.selected_file_type)
        if filename:
            return os.path.join(server_path, filename)
        return None
    
    def _get_internal_file_type(self) -> str:
        """Convertir tipo de archivo externo a tipo interno"""
        type_mapping = {
            "ini": "server_settings",
            "lua": "sandbox_vars",
            "spawn_regions": "spawn_regions",
            "spawn_points": "server_rules"
        }
        return type_mapping.get(self.selected_file_type, "server_settings")
    
    def _get_template_content(self) -> str:
        """Obtener contenido de plantilla para el tipo de archivo actual"""
        templates = {
            "ini": """# Configuración del Servidor Project Zomboid
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
            "lua": """-- Configuración de SandBox Variables
-- Generado automáticamente por PZ Server Administrator

-- Configuración básica del mundo
SandBoxVars.Speed = 2,
SandBoxVars.Zombies = 4,
SandBoxVars.Distribution = 1,
SandBoxVars.Survivors = 4,
SandBoxVars.DayLength = 1,
SandBoxVars.StartYear = 1,
SandBoxVars.StartMonth = 7,
SandBoxVars.StartDay = 9,
SandBoxVars.StartTime = 2,
SandBoxVars.WaterShut = 2,
SandBoxVars.ElecShut = 2,
SandBoxVars.WaterShutModifier = 14,
SandBoxVars.ElecShutModifier = 14,
SandBoxVars.FoodLoot = 2,
SandBoxVars.CannedFoodLoot = 2,
SandBoxVars.LiteratureLoot = 2,
SandBoxVars.SurvivalGearsLoot = 2,
SandBoxVars.MedicalLoot = 2,
SandBoxVars.WeaponLoot = 2,
SandBoxVars.RangedWeaponLoot = 2,
SandBoxVars.AmmoLoot = 2,
SandBoxVars.MechanicsLoot = 2,
SandBoxVars.OtherLoot = 2,
SandBoxVars.Temperature = 3,
SandBoxVars.Rain = 3,
SandBoxVars.ErosionSpeed = 3,
SandBoxVars.ErosionDays = 0,
SandBoxVars.XpMultiplier = 1.0,
SandBoxVars.ZombieAttractionMultiplier = 1.0,
SandBoxVars.VehicleEasyUse = false,
SandBoxVars.Farming = 3,
SandBoxVars.CompostTime = 2,
SandBoxVars.StatsDecrease = 3,
SandBoxVars.NatureAbundance = 3,
SandBoxVars.Alarm = 4,
SandBoxVars.LockedHouses = 6,
SandBoxVars.StarterKit = false,
SandBoxVars.Nutrition = true,
SandBoxVars.FoodRotSpeed = 3,
SandBoxVars.FridgeFactor = 3,
SandBoxVars.LootRespawn = 1,
SandBoxVars.LootSeenHours = 0,
SandBoxVars.WorldItemRemovalList = "Base.Hat,Base.Glasses,Base.Maggots",
SandBoxVars.HoursForWorldItemRemoval = 24.0,
SandBoxVars.ItemRemovalListBlacklistToggle = false,
SandBoxVars.TimeSinceApo = 1,
SandBoxVars.PlantResilience = 3,
SandBoxVars.PlantAbundance = 3,
SandBoxVars.EndRegen = 3,
SandBoxVars.Helicopter = 2,
SandBoxVars.MetaEvent = 2,
SandBoxVars.SleepingEvent = 1,
SandBoxVars.GeneratorSpawning = 3,
SandBoxVars.GeneratorFuelConsumption = 1.0,
SandBoxVars.SurvivorHouseChance = 3,
SandBoxVars.VehicleStoryChance = 3,
SandBoxVars.ZoneStoryChance = 3,
SandBoxVars.AnnotatedMapChance = 4,
SandBoxVars.CharacterFreePoints = 0,
SandBoxVars.ConstructionBonusPoints = 3,
SandBoxVars.NightDarkness = 3,
SandBoxVars.InjurySeverity = 2,
SandBoxVars.BoneFracture = true,
SandBoxVars.HoursForCorpseRemoval = 216.0,
SandBoxVars.DecayingCorpseHealthImpact = 3,
SandBoxVars.BloodLevel = 3,
SandBoxVars.ClothingDegradation = 3,
SandBoxVars.FireSpread = true,
SandBoxVars.DaysForRottenFoodRemoval = -1,
SandBoxVars.AllowExteriorGenerator = true,
SandBoxVars.MaxFogIntensity = 1,
SandBoxVars.MaxRainFxIntensity = 1,
SandBoxVars.EnableSnowOnGround = true,
SandBoxVars.MultiHitZombies = false,
SandBoxVars.RearVulnerability = 3,
SandBoxVars.AttackBlockMovements = true,
SandBoxVars.AllClothesUnlocked = false,"""
        }
        return templates.get(self.selected_file_type, "# Archivo de configuración")
    
    def _force_update(self):
        """Forzar actualización de la interfaz"""
        try:
            # Intentar actualizar desde el editor_container
            if hasattr(self.editor_container, 'page') and self.editor_container.page:
                self.editor_container.page.update()
                return
            
            # Intentar actualizar desde cualquier control hijo que tenga página
            controls_to_check = [
                self.ini_simple_editor,
                self.lua_simple_editor,
                self.advanced_text_editor,
                self.edit_mode_control,
                self.file_buttons_control
            ]
            
            for control in controls_to_check:
                if hasattr(control, 'page') and control.page:
                    control.page.update()
                    return
                # Verificar si el control tiene un contenedor con página
                if hasattr(control, 'container') and hasattr(control.container, 'page') and control.container.page:
                    control.container.page.update()
                    return
                # Verificar si el control tiene un control_container con página
                if hasattr(control, 'control_container') and hasattr(control.control_container, 'page') and control.control_container.page:
                    control.control_container.page.update()
                    return
        except Exception as e:
            print(f"Error al forzar actualización: {e}")
    
    def _on_save_click(self, e):
        """Manejar clic en botón guardar"""
        if not self.current_server_id:
            self._show_snack_bar(e.page, "No hay servidor seleccionado", ft.Colors.ERROR)
            return
        
        file_path = self._get_config_file_path()
        if not file_path:
            self._show_snack_bar(e.page, "No se pudo determinar la ruta del archivo", ft.Colors.ERROR)
            return
        
        try:
            # Crear directorio si no existe
            os.makedirs(os.path.dirname(file_path), exist_ok=True)
            
            # Obtener contenido según el editor activo
            if self.current_mode == "simple" and self.selected_file_type == "ini":
                content = self.ini_simple_editor.get_ini_content()
            elif self.current_mode == "simple" and self.selected_file_type == "lua":
                content = self.lua_simple_editor.get_lua_content()
            else:
                content = self.advanced_text_editor.get_content()
            
            # Guardar archivo
            with open(file_path, 'w', encoding='utf-8') as file:
                file.write(content)
            
            # Actualizar estado de botones de archivos
            servers = config_loader.get_all_servers()
            server_data = servers.get(self.current_server_id, {})
            server_path = server_data.get('server_path')
            server_name = server_data.get('name')
            self.file_buttons_control.update_server_path(server_path, server_name)
            
            self._show_snack_bar(e.page, f"Configuración guardada: {os.path.basename(file_path)}", ft.Colors.GREEN)
            
        except Exception as ex:
            self._show_snack_bar(e.page, f"Error al guardar: {str(ex)}", ft.Colors.ERROR)
    
    def _on_reset_click(self, e):
        """Manejar clic en botón restaurar"""
        self._load_file_content()
        self._show_snack_bar(e.page, "Configuración restaurada", ft.Colors.ORANGE)
        e.page.update()
    
    def _on_validate_click(self, e):
        """Manejar clic en botón validar"""
        # Implementar validación según el tipo de archivo
        if self.selected_file_type == "ini":
            self._validate_ini_syntax(e.page)
        elif self.selected_file_type == "lua":
            self._validate_lua_syntax(e.page)
        elif self.selected_file_type == "spawn_points":
            self._validate_json_syntax(e.page)
        else:
            self._show_snack_bar(e.page, "Validación no disponible para este tipo de archivo", ft.Colors.WARNING)
    
    def _validate_ini_syntax(self, page):
        """Validar sintaxis de archivo INI"""
        try:
            if self.current_mode == "simple":
                content = self.ini_simple_editor.get_ini_content()
            else:
                content = self.advanced_text_editor.get_content()
            
            # Validación básica de sintaxis INI
            import configparser
            
            # Verificar si el contenido tiene headers de sección
            has_sections = any(line.strip().startswith('[') and line.strip().endswith(']') 
                             for line in content.split('\n'))
            
            if has_sections:
                # Validar como INI normal con secciones
                config = configparser.ConfigParser(allow_no_value=True)
                config.read_string(content)
            else:
                # Validar como archivo de configuración sin secciones
                lines = content.strip().split('\n')
                for line in lines:
                    line = line.strip()
                    if line and not line.startswith('#') and '=' in line:
                        # Verificar que la línea tenga formato clave=valor válido
                        if line.count('=') < 1:
                            raise ValueError(f"Línea inválida: {line}")
            
            self._show_snack_bar(page, "Sintaxis INI válida", ft.Colors.GREEN)
        except Exception as e:
            self._show_snack_bar(page, f"Error de sintaxis INI: {str(e)}", ft.Colors.ERROR)
    
    def _validate_lua_syntax(self, page):
        """Validar sintaxis de archivo Lua (validación básica)"""
        content = self.advanced_text_editor.get_content()
        
        # Validación básica de sintaxis Lua
        lua_keywords = ['function', 'end', 'if', 'then', 'else', 'elseif', 'while', 'do', 'for', 'repeat', 'until', 'local', 'return']
        
        # Verificar balanceado de llaves y paréntesis
        open_braces = content.count('{')
        close_braces = content.count('}')
        open_parens = content.count('(')
        close_parens = content.count(')')
        
        if open_braces != close_braces:
            self._show_snack_bar(page, f"Error: Llaves desbalanceadas ({open_braces} abiertas, {close_braces} cerradas)", ft.Colors.ERROR)
            return
        
        if open_parens != close_parens:
            self._show_snack_bar(page, f"Error: Paréntesis desbalanceados ({open_parens} abiertos, {close_parens} cerrados)", ft.Colors.ERROR)
            return
        
        self._show_snack_bar(page, "Sintaxis Lua básica válida", ft.Colors.GREEN)
    
    def _validate_json_syntax(self, page):
        """Validar sintaxis de archivo JSON"""
        try:
            import json
            content = self.advanced_text_editor.get_content()
            json.loads(content)
            self._show_snack_bar(page, "Sintaxis JSON válida", ft.Colors.GREEN)
        except json.JSONDecodeError as e:
            self._show_snack_bar(page, f"Error de sintaxis JSON: {str(e)}", ft.Colors.ERROR)
    
    def _on_advanced_save(self, content: str):
        """Callback para guardar desde el editor avanzado"""
        # Este método se llama desde el editor avanzado cuando se presiona su botón guardar
        pass  # El guardado se maneja desde _on_save_click
    
    def _show_snack_bar(self, page, message: str, color):
        """Mostrar mensaje en snack bar"""
        if page:
            page.snack_bar = ft.SnackBar(
                content=ft.Text(message),
                bgcolor=color
            )
            page.snack_bar.open = True
            page.update()
    
    def set_server(self, server_id: str):
        """Establecer el servidor actual"""
        self.current_server_id = server_id
        self._update_server_info()
        servers = config_loader.get_all_servers()
        server_data = servers.get(server_id, {}) if server_id else {}
        server_path = server_data.get('server_path')
        server_name = server_data.get('name')
        self.file_buttons_control.update_server_path(server_path, server_name)
        self._update_editor_view()
    
    def refresh_for_selected_server(self):
        """Actualizar para el servidor seleccionado"""
        self.current_server_id = config_loader.get_selected_server()
        self._update_server_info()
        if self.current_server_id:
            servers = config_loader.get_all_servers()
            server_data = servers.get(self.current_server_id, {})
            server_path = server_data.get('server_path')
            server_name = server_data.get('name')
            self.file_buttons_control.update_server_path(server_path, server_name)
        self._update_editor_view()
    
    def set_edit_mode(self, mode: str):
        """Establecer el modo de edición (compatibilidad con main_layout)"""
        if mode in ["simple", "advanced"]:
            self.current_mode = mode
            self.edit_mode_control.set_mode(mode)
            self._update_editor_view()
    
    def set_file_type(self, file_type: str):
        """Establecer el tipo de archivo (compatibilidad con main_layout)"""
        if file_type in ["ini", "lua", "spawn_regions", "spawn_points"]:
            self.selected_file_type = file_type
            self.file_buttons_control.set_selected_file(file_type)
            self._load_file_content()
            self._update_editor_view()
    
    def get_control(self):
        """Obtener el control principal"""
        return ft.Column([
            # Título
            ft.Text(
                "Gestión de Configuraciones",
                size=24,
                weight=ft.FontWeight.BOLD
            ),
            
            # Información del servidor
            self.server_info_text,
            
            ft.Divider(),
            
            # Selector de modo de edición
            self.edit_mode_control.get_control(),
            
            ft.Container(height=10),
            
            # Editor principal
            self.editor_container,
            
            # Botones de acción
            ft.Row([
                self.save_button,
                self.reset_button,
                self.validate_button
            ], spacing=10)
        ], spacing=10, expand=True)
    
    def build(self):
        """Método de compatibilidad con la interfaz anterior"""
        return self.get_control()