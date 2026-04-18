{
	outputs = { self, nixpkgs }:
		let
		system = "x86_64-linux";
	pkgs = nixpkgs.legacyPackages.${system};
	libraries = with pkgs; [
		libGL
	];
	in {
		devShells.${system}.default = pkgs.mkShell {
			LD_LIBRARY_PATH = pkgs.lib.makeLibraryPath libraries;

			UNITY = "/home/mesalon/Applications/Unity/Editor/6000.3.5f1/Editor/Unity";
		};
	};
}
# at-spi2-atk
# 	SDL2
# 	cairo
# 	fontconfig
# 	gtk3
# 	gdk-pixbuf
# 	glib
# 	libGL
# 	harfbuzz
# 	pango
# 	stdenv.cc.cc.lib
# 	udev
# 	zlib
# 	letmyxml2
# 	libX11
# 	libXcursor
# 	libXrandr
# 	libXi
# 	libxcb
# 	libxkbcommon
# 	vulkan-loader
# 	vulkan-tools
# 	vulkan-validation-layers
# 	wayland
# 	openssl
# 	libuuid
