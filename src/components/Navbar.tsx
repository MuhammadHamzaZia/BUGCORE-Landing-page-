import React from 'react';
import { Github } from 'lucide-react';

interface NavbarProps {
  onOpenDocs: () => void;
  onOpenApi: () => void;
  onOpenDownload: () => void;
}

export const Navbar: React.FC<NavbarProps> = ({
  onOpenDocs,
  onOpenApi,
}) => {
  return (
    <header className="w-full border-b border-gray-200 bg-white sticky top-0 z-40">
      <div className="mx-auto flex h-14 sm:h-18 w-full max-w-[min(100%-1rem,1440px)] 2xl:max-w-[min(100%-3rem,1640px)] items-center justify-between px-2.5 sm:px-6 lg:px-8">
        {/* Brand / Logo - Sized properly on mobile and desktop */}
        <div className="flex items-center shrink-0">
          <a
            href="/"
            className="flex items-center gap-2 sm:gap-3 text-gray-950 font-bold font-mono text-base sm:text-xl select-none"
          >
            <img
              src="/logo.svg"
              alt="BUGCORE logo"
              className="h-7 w-7 sm:h-10 sm:w-10 object-contain drop-shadow-xs shrink-0"
            />
            <span className="font-extrabold tracking-tight sm:tracking-wider">BUGCORE</span>
          </a>
        </div>

        {/* Right Nav: Documentation | API | [ GitHub ] */}
        <div className="flex items-center gap-1.5 sm:gap-4 text-xs sm:text-sm font-sans font-semibold shrink-0">
          <button
            onClick={onOpenDocs}
            className="text-gray-800 hover:text-black transition-colors cursor-pointer py-1 px-1.5 sm:px-2 rounded-xs hover:bg-gray-100"
          >
            <span className="hidden sm:inline">Documentation</span>
            <span className="inline sm:hidden">Docs</span>
          </button>

          <span className="text-gray-300 font-normal select-none">|</span>

          <button
            onClick={onOpenApi}
            className="text-gray-800 hover:text-black transition-colors cursor-pointer py-1 px-1.5 sm:px-2 rounded-xs hover:bg-gray-100"
          >
            API
          </button>

          <span className="text-gray-300 font-normal select-none">|</span>

          <a
            href="https://github.com/MuhammadHamzaZia/BUGCORE"
            target="_blank"
            rel="noreferrer"
            aria-label="GitHub Repository"
            className="border border-gray-300 bg-white hover:bg-gray-50 text-gray-950 px-2 sm:px-3 py-1 sm:py-1.5 font-mono text-[11px] sm:text-xs font-bold rounded-xs transition-colors cursor-pointer inline-flex items-center gap-1 sm:gap-1.5 whitespace-nowrap shadow-2xs"
          >
            <Github className="w-3.5 h-3.5 sm:w-4 sm:h-4 text-gray-800 shrink-0" />
            <span className="hidden sm:inline">[ GitHub ]</span>
            <span className="inline sm:hidden">Git</span>
          </a>
        </div>
      </div>
    </header>
  );
};
