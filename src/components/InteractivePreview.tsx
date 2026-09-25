import React, { useState } from 'react';
import { Maximize2, ExternalLink } from 'lucide-react';

export const InteractivePreview: React.FC = () => {
  const [isModalOpen, setIsModalOpen] = useState(false);

  return (
    <>
      <div className="w-full border border-gray-300 bg-white shadow-xs rounded-xs overflow-hidden">
        {/* Top Window Bar */}
        <div className="flex items-center justify-between border-b border-gray-200 bg-gray-50 px-3.5 sm:px-4 py-2.5 text-xs">
          <div className="flex items-center gap-2.5">
            <img
              src={`${import.meta.env.BASE_URL}logo.svg`}
              alt="BUGCORE"
              className="h-5 w-5 object-contain"
            />
            <span className="font-mono text-gray-900 text-xs font-bold tracking-tight">
              ( UI Screenshot )
            </span>
          </div>

          <div className="flex items-center gap-2 font-mono text-xs font-semibold text-gray-600">
            <button
              onClick={() => setIsModalOpen(true)}
              className="hover:text-black flex items-center gap-1 cursor-pointer transition-colors py-0.5 px-1.5"
              title="View full resolution"
            >
              <Maximize2 className="w-3.5 h-3.5 stroke-[2.2]" />
              <span>[ expand ]</span>
            </button>
          </div>
        </div>

        {/* Screenshot Image Container - Fluid Liquid Scaling */}
        <div
          className="relative bg-white cursor-pointer group overflow-hidden w-full flex items-center justify-center"
          onClick={() => setIsModalOpen(true)}
        >
          <img
            src={`${import.meta.env.BASE_URL}screenshot.png`}
            alt="BUGCORE Defect Tracker Screenshot"
            className="w-full h-auto max-h-[580px] object-contain block transition-transform duration-300 group-hover:scale-[1.01]"
            loading="eager"
          />
          <div className="absolute inset-0 bg-gray-950/0 group-hover:bg-gray-950/5 transition-colors flex items-center justify-center pointer-events-none">
            <span className="opacity-0 group-hover:opacity-100 transition-opacity bg-gray-950/90 text-white font-mono text-xs font-semibold px-3 py-1.5 rounded-xs shadow-sm flex items-center gap-1.5">
              <ExternalLink className="w-3.5 h-3.5" />
              Click to enlarge
            </span>
          </div>
        </div>
      </div>

      {/* Fullscreen Lightbox Modal */}
      {isModalOpen && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center p-2 sm:p-4 md:p-6 bg-gray-950/80 backdrop-blur-xs cursor-zoom-out"
          onClick={() => setIsModalOpen(false)}
        >
          <div
            className="relative w-full max-w-[min(96vw,1440px)] max-h-[94vh] bg-white border border-gray-300 p-2 sm:p-3 cursor-default flex flex-col rounded-xs shadow-2xl"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="flex items-center justify-between border-b border-gray-200 pb-2.5 mb-2 px-1 text-xs font-mono text-gray-700 shrink-0">
              <div className="flex items-center gap-2.5">
                <img src={`${import.meta.env.BASE_URL}logo.svg`} alt="BUGCORE" className="h-6 w-6 object-contain" />
                <span className="font-extrabold text-sm text-gray-950">BUGCORE UI Screenshot</span>
              </div>
              <button
                onClick={() => setIsModalOpen(false)}
                className="hover:text-black font-bold px-2.5 py-1 border border-gray-300 bg-gray-50 cursor-pointer text-xs rounded-xs"
              >
                ✕ Close [Esc]
              </button>
            </div>
            <div className="overflow-auto max-h-[82vh] overscroll-contain flex justify-center">
              <img
                src={`${import.meta.env.BASE_URL}screenshot.png`}
                alt="BUGCORE Defect Tracker Full Resolution"
                className="w-full h-auto max-w-full object-contain block"
              />
            </div>
          </div>
        </div>
      )}
    </>
  );
};
