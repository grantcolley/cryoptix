import React from "react";
import "./CryoptixLogo.css";

type CryoptixLogoVariant = "logo" | "icon" | "wordmark";
type CryoptixLogoTheme = "auto" | "light" | "dark";

type CryoptixLogoProps = React.SVGProps<SVGSVGElement> & {
  variant?: CryoptixLogoVariant;
  theme?: CryoptixLogoTheme;
  title?: string;
};

const wordmarkPath = `M 615.00 77.50 L 593.00 76.50 L 592.50 75.00 L 630.50 38.00 L 594.50 1.00 L 619.00 0.50 L 643.00 25.50 L 667.00 0.50 L 692.00 -0.50 L 692.50 1.00 L 655.50 38.00 L 692.50 76.00 L 668.00 76.50 L 643.00 49.50 L 615.00 77.50 Z M 75.00 77.50 L 18.00 76.50 L 8.00 72.50 L 3.50 68.00 L -0.50 57.00 L -0.50 20.00 L 3.50 9.00 L 8.00 4.50 L 17.00 0.50 L 26.00 -0.50 L 75.50 0.00 L 75.00 15.50 L 31.00 14.50 L 24.00 15.50 L 20.50 19.00 L 19.50 23.00 L 19.50 55.00 L 22.00 59.50 L 27.00 61.50 L 76.00 61.50 L 76.50 76.00 L 75.00 77.50 Z M 177.00 76.50 L 153.00 76.50 L 131.00 50.50 L 113.00 50.50 L 112.50 76.00 L 94.00 76.50 L 93.50 0.00 L 136.00 -0.50 L 154.00 0.50 L 163.00 3.50 L 169.50 10.00 L 172.50 17.00 L 172.50 33.00 L 166.00 43.50 L 152.50 49.00 L 177.00 76.50 Z M 232.00 76.50 L 213.50 76.00 L 213.50 48.00 L 178.00 0.50 L 198.00 -0.50 L 223.00 31.50 L 248.00 0.50 L 268.50 1.00 L 232.50 48.00 L 232.00 76.50 Z M 338.00 77.50 L 295.00 77.50 L 282.00 72.50 L 278.50 69.00 L 274.50 59.00 L 274.50 17.00 L 277.50 9.00 L 282.00 4.50 L 296.00 -0.50 L 338.00 -0.50 L 351.00 3.50 L 357.50 10.00 L 360.50 18.00 L 359.50 62.00 L 356.50 68.00 L 351.00 73.50 L 338.00 77.50 Z M 397.00 76.50 L 378.50 76.00 L 378.50 0.00 L 438.00 0.50 L 449.00 4.50 L 454.50 10.00 L 457.50 19.00 L 457.50 34.00 L 454.50 43.00 L 448.00 49.50 L 439.00 52.50 L 398.00 52.50 L 397.00 76.50 Z M 514.00 76.50 L 493.50 76.00 L 494.50 16.00 L 464.00 15.50 L 465.00 0.50 L 541.50 1.00 L 542.50 15.00 L 541.00 16.50 L 514.50 16.00 L 514.00 76.50 Z M 578.00 76.50 L 558.50 76.00 L 559.00 0.50 L 564.00 -0.50 L 578.50 1.00 L 578.00 76.50 Z M 332.50 62.00 L 337.00 60.50 L 340.50 55.00 L 340.50 21.00 L 338.50 17.00 L 334.00 14.50 L 298.00 15.50 L 294.50 19.00 L 293.50 25.00 L 293.50 52.00 L 295.50 59.00 L 303.00 62.50 L 332.50 62.00 Z M 432.50 37.00 L 438.50 31.00 L 438.50 22.00 L 435.00 16.50 L 430.00 14.50 L 397.50 15.00 L 398.00 37.50 L 432.50 37.00 Z M 142.50 36.00 L 150.00 34.50 L 153.50 30.00 L 153.50 21.00 L 151.00 16.50 L 146.00 14.50 L 112.50 15.00 L 113.00 36.50 L 142.50 36.00 Z`;

function getClassName(theme: CryoptixLogoTheme, className?: string) {
  return [
    "cryoptix-logo",
    theme === "auto" ? "cryoptix-logo--auto" : "",
    theme === "light" ? "cryoptix-logo--light" : "",
    theme === "dark" ? "cryoptix-logo--dark" : "",
    className ?? "",
  ]
    .filter(Boolean)
    .join(" ");
}

function CryoptixIconMark() {
  return <g className="cryoptix-logo__mark">
      <path d="M 91 372 C 35 333 8 277 11 215 C 15 126 82 48 170 22 C 240 1 319 16 375 63" fill="none" stroke="currentColor" strokeWidth="25" strokeLinecap="round" strokeLinejoin="round" />
      <path d="M 109 391 C 161 425 229 435 292 413 C 376 385 434 310 442 222 C 444 203 443 184 440 166" fill="none" stroke="currentColor" strokeWidth="25" strokeLinecap="round" strokeLinejoin="round" />
      <rect x="286" y="123" width="39" height="104" rx="7" fill="currentColor" /><rect x="301" y="88" width="8" height="179" rx="4" fill="currentColor" />
      <rect x="352" y="94" width="39" height="95" rx="7" fill="currentColor" /><rect x="367" y="62" width="8" height="162" rx="4" fill="currentColor" />
      <rect x="218" y="172" width="39" height="96" rx="7" fill="currentColor" /><rect x="233" y="142" width="8" height="167" rx="4" fill="currentColor" />
      <rect x="151" y="222" width="39" height="87" rx="7" fill="currentColor" /><rect x="167" y="190" width="8" height="165" rx="4" fill="currentColor" />
      <rect x="87" y="275" width="39" height="61" rx="7" fill="currentColor" /><rect x="102" y="243" width="8" height="112" rx="4" fill="currentColor" />
    </g>;
}

function CryoptixWordmark() {
  return <path fill="currentColor" fillRule="evenodd" d={wordmarkPath} />;
}

export function CryoptixLogo({
  variant = "logo",
  theme = "auto",
  title = "Cryoptix",
  className,
  ...props
}: CryoptixLogoProps) {
  const viewBox =
    variant === "icon" ? "0 0 453 435" : variant === "wordmark" ? "0 0 693 78" : "0 0 702 517";

  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      viewBox={viewBox}
      role="img"
      aria-label={title}
      className={getClassName(theme, className)}
      {...props}
    >
      <title>{title}</title>
      {variant === "icon" ? <CryoptixIconMark /> : null}
      {variant === "wordmark" ? <CryoptixWordmark /> : null}
      {variant === "logo" ? (
        <>
          <g transform="translate(143 0)">
            <CryoptixIconMark />
          </g>
          <g transform="translate(0 438)">
            <CryoptixWordmark />
          </g>
        </>
      ) : null}
    </svg>
  );
}
