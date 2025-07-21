import flet as ft
import os
import json
from typing import Optional, Dict, Any

class LuaSimpleEditorControl:
    """Control para edición simple de archivos SandBoxVars.lua con interfaz amigable"""
    
    def __init__(self):
        self.config_data = {}
        self.controls_map = {}  # Mapeo de controles para obtener valores
        self.help_data = {}  # Datos de ayuda para parámetros
        self.container = ft.Container()
        self._load_help_data()
        self._create_editor()
    
    def _create_editor(self):
        """Crear el editor simple inicial"""
        self.container = ft.Container(
            content=ft.Column([
                ft.Container(
                    content=ft.Text(
                        "No hay contenido para mostrar",
                        text_align=ft.TextAlign.CENTER,
                        color=ft.Colors.ON_SURFACE_VARIANT
                    ),
                    alignment=ft.alignment.center,
                    height=200
                )
            ], scroll=ft.ScrollMode.AUTO),
            padding=10,
            border=ft.border.all(1, ft.Colors.OUTLINE),
            border_radius=8,
            expand=True
        )
    
    def _load_help_data(self):
        """Cargar datos de ayuda desde parameter_help.json"""
        try:
            # Buscar el archivo en el directorio assets
            current_dir = os.path.dirname(os.path.abspath(__file__))
            assets_dir = os.path.join(os.path.dirname(current_dir), 'assets')
            help_file_path = os.path.join(assets_dir, 'parameter_help.json')
            
            if os.path.exists(help_file_path):
                with open(help_file_path, 'r', encoding='utf-8') as file:
                    data = json.load(file)
                    self.help_data = data.get('sandbox_vars', {})
            else:
                print(f"Archivo de ayuda no encontrado: {help_file_path}")
                self.help_data = {}
        except Exception as e:
            print(f"Error cargando datos de ayuda: {e}")
            self.help_data = {}
    
    def load_lua_content(self, content: str):
        """Cargar contenido Lua y crear controles de edición"""
        if not content or content.strip() == "":
            self._create_editor()
            return
        
        try:
            self.config_data = {}
            self.controls_map = {}
            
            # Parsear el contenido Lua para extraer variables SandBox
            self._parse_sandbox_vars(content)
            
            if self.config_data:
                self._create_controls()
            else:
                self._create_editor()
                
        except Exception as e:
            print(f"Error cargando contenido Lua: {e}")
            self._create_editor()
    
    def _parse_sandbox_vars(self, content: str):
        """Parsear contenido Lua para extraer variables SandBox"""
        lines = content.split('\n')
        current_section = "SandBoxVars"
        
        for line in lines:
            line = line.strip()
            
            # Ignorar comentarios y líneas vacías
            if not line or line.startswith('--'):
                continue
            
            # Buscar asignaciones de variables
            if '=' in line and not line.startswith('SandBoxVars'):
                # Extraer nombre y valor de la variable
                parts = line.split('=', 1)
                if len(parts) == 2:
                    var_name = parts[0].strip()
                    var_value = parts[1].strip()
                    
                    # Limpiar el nombre de la variable (remover SandBoxVars. si existe)
                    if var_name.startswith('SandBoxVars.'):
                        var_name = var_name[12:]  # Remover 'SandBoxVars.'
                    
                    # Limpiar el valor (remover comas y espacios)
                    var_value = var_value.rstrip(',').strip()
                    
                    # Remover comillas si existen
                    if var_value.startswith('"') and var_value.endswith('"'):
                        var_value = var_value[1:-1]
                    
                    # Guardar en config_data
                    if current_section not in self.config_data:
                        self.config_data[current_section] = {}
                    
                    self.config_data[current_section][var_name] = var_value
    
    def _create_controls(self):
        """Crear controles de edición basados en los datos del Lua"""
        if not self.config_data:
            self._create_editor()
            return
        
        # Crear lista de controles organizados por secciones
        sections = []
        
        for section_name, section_data in self.config_data.items():
            if not section_data:
                continue
            
            # Crear título de sección
            section_title = ft.Container(
                content=ft.Text(
                    section_name,
                    size=18,
                    weight=ft.FontWeight.BOLD,
                    color=ft.Colors.PRIMARY
                ),
                margin=ft.margin.only(top=20, bottom=10)
            )
            sections.append(section_title)
            
            # Crear controles para cada parámetro en la sección
            for key, value in section_data.items():
                config_row = self._create_config_row(section_name, key, value)
                sections.append(config_row)
        
        # Actualizar el contenedor principal
        self.container = ft.Container(
            content=ft.Column(
                sections,
                scroll=ft.ScrollMode.AUTO,
                spacing=5
            ),
            padding=10,
            border=ft.border.all(1, ft.Colors.OUTLINE),
            border_radius=8,
            expand=True
        )
    
    def _create_config_row(self, section: str, key: str, value: str):
        """Crear una fila de configuración con etiqueta y control de entrada"""
        control_key = f"{section}.{key}"
        
        # Crear el control apropiado según el tipo de valor
        input_control = self._create_input_control(key, value, control_key)
        
        # Guardar referencia del control
        self.controls_map[control_key] = input_control
        
        # Crear icono de ayuda si hay información disponible
        help_icon = self._create_help_icon(key)
        
        # Crear la fila con label, control de entrada e icono de ayuda
        row_controls = [
            ft.Container(
                content=ft.Text(
                    key,
                    size=14,
                    weight=ft.FontWeight.W_500,
                    color=ft.Colors.ON_SURFACE
                ),
                width=200,
                alignment=ft.alignment.center_left
            ),
            ft.Container(
                content=input_control,
                width=300,
                alignment=ft.alignment.center_left
            )
        ]
        
        if help_icon:
            row_controls.append(
                ft.Container(
                    content=help_icon,
                    width=40,
                    alignment=ft.alignment.center
                )
            )
        
        return ft.Container(
            content=ft.Row(
                row_controls,
                alignment=ft.MainAxisAlignment.START,
                spacing=10
            ),
            padding=ft.padding.symmetric(vertical=5, horizontal=10),
            border=ft.border.all(0.5, ft.Colors.OUTLINE_VARIANT),
            border_radius=4,
            margin=ft.margin.symmetric(vertical=2)
        )
    
    def _create_help_icon(self, key: str) -> Optional[ft.IconButton]:
        """Crear icono de ayuda con tooltip para un parámetro"""
        help_info = self.help_data.get(key)
        if not help_info:
            return None
        
        # Crear el tooltip con la información de ayuda
        tooltip_text = f"{help_info.get('title', key)}\n\n{help_info.get('description', 'Sin descripción disponible.')}"
        
        return ft.IconButton(
            icon=ft.Icons.HELP_OUTLINE,
            icon_size=16,
            icon_color=ft.Colors.PRIMARY,
            tooltip=tooltip_text,
            style=ft.ButtonStyle(
                padding=ft.padding.all(4),
                shape=ft.CircleBorder()
            )
        )
    
    def _create_input_control(self, key: str, value: str, control_key: str):
        """Crear el control de entrada apropiado según el tipo de configuración"""
        key_lower = key.lower()
        value_lower = value.lower().strip()
        
        # Valores booleanos
        if value_lower in ['true', 'false']:
            return ft.Switch(
                value=value_lower == 'true',
                data=control_key,
                label=value_lower.title()
            )
        
        # Valores numéricos
        try:
            # Intentar convertir a número
            if '.' in value:
                float(value)
                return ft.TextField(
                    value=value,
                    data=control_key,
                    width=200,
                    keyboard_type=ft.KeyboardType.NUMBER,
                    input_filter=ft.NumbersOnlyInputFilter()
                )
            else:
                int(value)
                return ft.TextField(
                    value=value,
                    data=control_key,
                    width=200,
                    keyboard_type=ft.KeyboardType.NUMBER,
                    input_filter=ft.NumbersOnlyInputFilter()
                )
        except ValueError:
            pass
        
        # Valores de texto por defecto
        return ft.TextField(
            value=value,
            data=control_key,
            width=200
        )
    
    def get_lua_content(self) -> str:
        """Obtener el contenido Lua desde los controles de edición"""
        if not self.controls_map:
            return ""
        
        # Construir el contenido Lua
        lua_lines = [
            "-- Configuración de SandBox Variables",
            "-- Generado automáticamente por PZ Server Administrator",
            ""
        ]
        
        # Agrupar por secciones
        sections = {}
        for control_key, control in self.controls_map.items():
            if '.' in control_key:
                section, key = control_key.split('.', 1)
                if section not in sections:
                    sections[section] = {}
                
                # Obtener valor del control
                if isinstance(control, ft.Switch):
                    value = "true" if control.value else "false"
                elif isinstance(control, ft.TextField):
                    value = control.value or ""
                else:
                    value = str(control.value) if hasattr(control, 'value') else ""
                
                sections[section][key] = value
        
        # Generar contenido Lua
        for section_name, section_data in sections.items():
            if section_data:
                lua_lines.append(f"-- {section_name}")
                for key, value in section_data.items():
                    # Formatear valor según tipo
                    if value.lower() in ['true', 'false']:
                        lua_lines.append(f"SandBoxVars.{key} = {value.lower()},")
                    elif value.isdigit() or (value.replace('.', '').isdigit() and value.count('.') <= 1):
                        lua_lines.append(f"SandBoxVars.{key} = {value},")
                    else:
                        lua_lines.append(f'SandBoxVars.{key} = "{value}",') 
                lua_lines.append("")
        
        return "\n".join(lua_lines)
    
    def get_control(self):
        """Obtener el control principal"""
        return self.container
    
    def clear(self):
        """Limpiar el editor"""
        self.config_data = {}
        self.controls_map = {}
        self._create_editor()