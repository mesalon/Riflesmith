{
	outputs = { self, nixpkgs }:
		let
		system = "x86_64-linux";
	pkgs = nixpkgs.legacyPackages.${system};
	libraries = with pkgs; [
			at-spi2-atk
			cairo
			fontconfig
			gtk3
			gdk-pixbuf
			glib
			libGL
			harfbuzz
			pango
			libgcc
			udev
			libx11
			libxcursor
			libxml2
			libxrandr
			zlib
			];
	in {
		devShells.${system}.default = pkgs.mkShell {
			packages = libraries;
			NIX_LD_LIBRARY_PATH = pkgs.lib.makeLibraryPath libraries;
			NIX_LD = pkgs.lib.fileContents "${pkgs.stdenv.cc}/nix-support/dynamic-linker";
			UNITY = "/home/mesalon/Applications/Unity/Editor/6000.3.5f1/Editor/Unity";
		};
	};
}
