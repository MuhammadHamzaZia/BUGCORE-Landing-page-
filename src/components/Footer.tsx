import React from 'react';

interface FooterProps {
  onOpenDownload: () => void;
  onOpenDocs: () => void;
  onOpenApi: () => void;
}

export const Footer: React.FC<FooterProps> = ({
  onOpenApi,
}) => {
  return (
    <footer className="border-t border-gray-200 bg-gray-50 py-7 text-xs font-mono text-gray-700">
      <div className="mx-auto w-full max-w-[min(100%-1.5rem,1440px)] 2xl:max-w-[min(100%-3rem,1640px)] px-3 sm:px-6 lg:px-8">
        <div className="flex flex-col sm:flex-row items-center justify-between gap-4">
          {/* Left statement with bigger logo */}
          <div className="flex items-center gap-2.5 text-gray-800 flex-wrap justify-center sm:justify-start font-medium">
            <img
              src={`${import.meta.env.BASE_URL}logo.svg`}
              alt="BUGCORE"
              className="h-6 w-6 object-contain inline-block shrink-0 drop-shadow-2xs"
            />
            <span className="font-extrabold text-gray-950 text-sm">BUGCORE</span>
            <span className="font-semibold">© 2026.</span>
            <span className="font-semibold text-gray-600">MIT License.</span>
          </div>

          {/* Right links */}
          <div className="flex items-center gap-3 sm:gap-5 text-gray-800 font-sans text-xs sm:text-sm font-semibold">
            <a
              href="https://github.com/MuhammadHamzaZia/BUGCORE"
              target="_blank"
              rel="noreferrer"
              className="hover:text-black transition-colors cursor-pointer py-1"
            >
              GitHub
            </a>
            <span className="text-gray-300 font-normal select-none">|</span>
            <a
              href="https://github.com/MuhammadHamzaZia/BUGCORE/issues"
              target="_blank"
              rel="noreferrer"
              className="hover:text-black transition-colors cursor-pointer py-1"
            >
              Issues
            </a>
            <span className="text-gray-300 font-normal select-none">|</span>
            <button
              onClick={onOpenApi}
              className="hover:text-black transition-colors cursor-pointer py-1"
            >
              Security
            </button>
          </div>
        </div>
      </div>
    </footer>
  );
};
