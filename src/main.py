# Archivo main.py - Punto de entrada alternativo
# Redirige a app.py para mantener compatibilidad

from app import main
import flet as ft

if __name__ == "__main__":
    ft.app(target=main, view=ft.WEB_BROWSER)
