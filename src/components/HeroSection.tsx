import React from 'react';
import { Download, Github } from 'lucide-react';
import { InteractivePreview } from './InteractivePreview';

interface HeroSectionProps {
  onOpenDownload?: () => void;
  onOpenDocs?: () => void;
}

export const HeroSection: React.FC<HeroSectionProps> = ({
  onOpenDocs,
}) => {
  const handleDownloadZip = (e: React.MouseEvent<HTMLAnchorElement>) => {
    // Normal anchor download works, but we also ensure programmatic trigger
    const link = document.createElement('a');
    link.href = '/BugCore.zip';
    link.setAttribute('download', 'BugCore.zip');
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return (
    <section className="bg-white py-[clamp(2.5rem,5vw,5rem)]">
      <div className="mx-auto w-full max-w-[min(100%-1.5rem,1440px)] 2xl:max-w-[min(100%-3rem,1640px)] px-3 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 lg:gap-10 xl:gap-14 items-center">
          {/* Left Column: Left-aligned hero content */}
          <div className="lg:col-span-5 flex flex-col justify-center">
            <h1 className="text-[clamp(2.25rem,4.2vw,3.5rem)] font-extrabold tracking-tight text-gray-950 leading-[1.12]">
              Self-hosted issue
              <br />
              tracking.
            </h1>

            <p className="mt-4 sm:mt-6 text-[clamp(1.05rem,1.4vw,1.2rem)] font-medium text-gray-750 text-gray-700 leading-relaxed max-w-xl">
              An open-source ASP.NET Core defect tracker with native Slack
              integrations and granular role-based access control.
            </p>

            {/* Action buttons: 1st black button (GitHub Repo) & 2nd white button (BugCore.zip download) */}
            <div className="mt-6 sm:mt-8 flex flex-col sm:flex-row lg:flex-col xl:flex-row items-stretch sm:items-center lg:items-stretch xl:items-center gap-3 w-full sm:w-auto">
              {/* 1st Black Button: GitHub Repository */}
              <a
                href="https://github.com/MuhammadHamzaZia/BUGCORE"
                target="_blank"
                rel="noreferrer"
                className="bg-gray-950 hover:bg-black text-white font-semibold text-sm px-6 py-3 rounded-xs inline-flex items-center justify-center gap-2 border border-gray-950 transition-colors cursor-pointer min-h-[44px] shadow-xs select-none"
              >
                <Github className="w-4 h-4 shrink-0 stroke-[2.2]" />
                <span>[ GitHub Repository ]</span>
              </a>

              {/* 2nd White Button: Download BugCore.zip from public folder */}
              <a
                href="/BugCore.zip"
                download="BugCore.zip"
                onClick={handleDownloadZip}
                className="bg-white hover:bg-gray-50 text-gray-950 font-semibold text-sm px-6 py-3 rounded-xs inline-flex items-center justify-center gap-2 border border-gray-300 transition-colors cursor-pointer min-h-[44px] select-none"
              >
                <Download className="w-4 h-4 text-gray-800 shrink-0 stroke-[2.2]" />
                <span>[ Download BugCore.zip ]</span>
              </a>
            </div>

            {/* Secondary Documentation Link */}
            {onOpenDocs && (
              <div className="mt-3 text-xs text-gray-500 font-medium">
                Looking for configuration guides?{' '}
                <button
                  type="button"
                  onClick={onOpenDocs}
                  className="text-gray-800 underline hover:text-black font-semibold cursor-pointer"
                >
                  Read documentation &rarr;
                </button>
              </div>
            )}
          </div>

          {/* Right Column: Screenshot */}
          <div className="lg:col-span-7 w-full flex justify-center">
            <InteractivePreview />
          </div>
        </div>
      </div>
    </section>
  );
};
