import flet as ft
from layouts.main_layout import MainLayout


def main(page: ft.Page):
    """
    Punto de entrada principal de la aplicación PZ Server Administrator
    """
    page.title = "Project Zomboid Server Administrator"
    page.theme_mode = ft.ThemeMode.DARK
    page.window_width = 1200
    page.window_height = 800
    page.window_min_width = 800
    page.window_min_height = 600
    
    # Inicializar el layout principal
    main_layout = MainLayout(page)
    page.add(main_layout.build())


if __name__ == "__main__":
    ft.app(target=main, view=ft.WEB_BROWSER)