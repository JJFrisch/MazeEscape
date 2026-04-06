<!--
  MazeIntroOverlay: Full-screen animated intro overlay shown before gameplay begins.
  Fades in with a large title + subtitle, cycles through loading phrases, then shows
  "Done ✓" and waits for a click-anywhere dismissal before calling ondismiss.
-->
<script lang="ts">
	import { onMount, onDestroy } from 'svelte';

	let {
		title,
		subtitle = '',
		phrases,
		accentColor = '#38bdf8',
		ondismiss
	}: {
		title: string;
		subtitle?: string;
		phrases: string[];
		accentColor?: string;
		ondismiss: () => void;
	} = $props();

	const PHRASE_INTERVAL_MS = 620;

	let phraseIndex = $state(0);
	let isDone = $state(false);
	let isExiting = $state(false);
	let dismissed = false;

	const currentPhrase = $derived(phrases[phraseIndex] ?? '');

	let phraseTimer: ReturnType<typeof setInterval>;

	function dismiss() {
		if (!isDone || dismissed) return;
		dismissed = true;
		isExiting = true;
		setTimeout(() => ondismiss(), 380);
	}

	onMount(() => {
		// Capture length as a plain value so the interval callback has no
		// dependency on Svelte 5 reactive signals or props references.
		const total = phrases.length;
		let step = 0;
		phraseTimer = setInterval(() => {
			step++;
			if (step < total) {
				phraseIndex = step;
			} else {
				clearInterval(phraseTimer);
				isDone = true;
			}
		}, PHRASE_INTERVAL_MS);
	});

	onDestroy(() => {
		clearInterval(phraseTimer);
	});
</script>

<!-- svelte-ignore a11y_click_events_have_key_events -->
<!-- svelte-ignore a11y_no_static_element_interactions -->
<div
	class="intro-overlay"
	class:exiting={isExiting}
	onclick={dismiss}
	style="--accent: {accentColor}"
>
	<div class="intro-inner">
		<p class="intro-subtitle">{subtitle}</p>
		<h1 class="intro-title">{title}</h1>

		<div class="phrase-area">
			{#if isDone}
				<span class="phrase-done">Done ✓</span>
			{:else}
				{#key phraseIndex}
					<span class="phrase-text">{currentPhrase}</span>
				{/key}
				<span class="dots" aria-hidden="true">
					<span class="dot"></span>
					<span class="dot"></span>
					<span class="dot"></span>
				</span>
			{/if}
		</div>

		{#if isDone}
			<p class="tap-hint">Tap anywhere to begin</p>
		{/if}
	</div>
</div>

<style>
	.intro-overlay {
		position: fixed;
		inset: 0;
		z-index: 300;
		display: flex;
		align-items: center;
		justify-content: center;
		cursor: pointer;
		/* Dark base with subtle accent-tinted grid texture */
		background-color: rgba(4, 8, 22, 0.95);
		background-image:
			repeating-linear-gradient(
				0deg,
				transparent,
				transparent 39px,
				color-mix(in srgb, var(--accent) 7%, transparent) 40px
			),
			repeating-linear-gradient(
				90deg,
				transparent,
				transparent 39px,
				color-mix(in srgb, var(--accent) 7%, transparent) 40px
			);
		animation: intro-fade-in 480ms cubic-bezier(0.16, 1, 0.3, 1) both;
	}

	.intro-overlay.exiting {
		animation: intro-fade-out 380ms ease forwards;
	}

	.intro-inner {
		text-align: center;
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 1.25rem;
		padding: 2rem;
		max-width: 640px;
		width: 100%;
		pointer-events: none; /* clicks pass to the overlay container */
	}

	.intro-subtitle {
		font-size: 0.8125rem;
		font-weight: 600;
		letter-spacing: 0.14em;
		text-transform: uppercase;
		color: var(--accent);
		opacity: 0.8;
		animation: content-rise 500ms 120ms cubic-bezier(0.16, 1, 0.3, 1) both;
	}

	.intro-title {
		font-family: var(--font-display);
		font-size: clamp(3rem, 11vw, 6rem);
		font-weight: 700;
		line-height: 1;
		color: #f0f6ff;
		text-shadow:
			0 0 60px color-mix(in srgb, var(--accent) 45%, transparent),
			0 2px 6px rgba(0, 0, 0, 0.5);
		animation: title-rise 600ms cubic-bezier(0.16, 1, 0.3, 1) both;
	}

	.phrase-area {
		min-height: 3rem;
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 0.5rem;
		margin-top: 0.25rem;
		animation: content-rise 500ms 200ms cubic-bezier(0.16, 1, 0.3, 1) both;
	}

	.phrase-text {
		font-size: 1.0625rem;
		color: rgba(240, 246, 255, 0.55);
		font-family: var(--font-mono);
		animation: phrase-swap 280ms ease both;
	}

	.phrase-done {
		font-size: 1.25rem;
		font-weight: 700;
		color: var(--accent);
		font-family: var(--font-display);
		letter-spacing: 0.04em;
		animation: phrase-swap 280ms ease both;
		text-shadow: 0 0 24px color-mix(in srgb, var(--accent) 50%, transparent);
	}

	.dots {
		display: flex;
		gap: 6px;
	}

	.dot {
		width: 6px;
		height: 6px;
		border-radius: 50%;
		background: var(--accent);
		animation: dot-pulse 1.2s ease-in-out infinite;
	}
	.dot:nth-child(2) { animation-delay: 0.2s; }
	.dot:nth-child(3) { animation-delay: 0.4s; }

	.tap-hint {
		font-size: 0.75rem;
		color: rgba(240, 246, 255, 0.35);
		letter-spacing: 0.08em;
		text-transform: uppercase;
		animation: content-rise 400ms 180ms cubic-bezier(0.16, 1, 0.3, 1) both;
	}

	/* ── Keyframes ─────────────────────────────────── */
	@keyframes intro-fade-in {
		from { opacity: 0; }
		to   { opacity: 1; }
	}

	@keyframes intro-fade-out {
		from { opacity: 1; }
		to   { opacity: 0; }
	}

	@keyframes title-rise {
		from { opacity: 0; transform: translateY(28px); }
		to   { opacity: 1; transform: translateY(0); }
	}

	@keyframes content-rise {
		from { opacity: 0; transform: translateY(14px); }
		to   { opacity: 1; transform: translateY(0); }
	}

	@keyframes phrase-swap {
		from { opacity: 0; transform: translateY(8px); }
		to   { opacity: 1; transform: translateY(0); }
	}

	@keyframes dot-pulse {
		0%, 100% { opacity: 0.25; transform: scale(0.75); }
		50%       { opacity: 0.9;  transform: scale(1.2); }
	}
</style>
