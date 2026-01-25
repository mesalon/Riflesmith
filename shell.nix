{ pkgs ? import (fetchTarball "https://github.com/NixOS/nixpkgs/archive/nixos-unstable.tar.gz") {} }:

let
unity-env = pkgs.buildFHSEnv {
	name = "unity-fhs-mesalon";
	targetPkgs = pkgs: (with pkgs; [
			libxml2
			SDL2
			libGL
			pkgs.xorg.libXi
			pkgs.xorg.libXext
			pkgs.xorg.libXcursor
			pkgs.xorg.libXrandr
			pkgs.xorg.libX11
			pkgs.xorg.libXrender
			pkgs.xorg.libXScrnSaver
			udev
			gtk3
			gdk-pixbuf
			glib
			zlib
			cairo
			pango
			fontconfig
			harfbuzz
			at-spi2-atk
			openssl
			libgcc

			openxr-loader
			vulkan-loader
			vulkan-tools
			]);

	profile = ''
		mkdir -p /tmp/unity-libs
		ln -sf /usr/lib/libxml2.so /tmp/unity-libs/libxml2.so.2
		export LD_LIBRARY_PATH=/tmp/unity-libs:/run/opengl-driver/lib:/usr/lib:/lib:$LD_LIBRARY_PATH
		export UNITY_PATH="/home/mesalon/Applications/Unity/Editor/6000.3.2f1/Editor/Unity"
		export SSL_CERT_FILE=/etc/ssl/certs/ca-bundle.crt
		export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

		export XR_LOADER_DEBUG=all
		export VK_LOADER_DEBUG=all

		export XR_RUNTIME_JSON=/home/mesalon/.config/openxr/1/active_runtime.json
		'';
	runScript = "bash";
};
in unity-env.env
