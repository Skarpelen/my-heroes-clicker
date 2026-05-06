type ToggleRowProps = {
  title: string
  description: string
  checked: boolean
  disabled?: boolean
  onChange: (checked: boolean) => void
}

export function ToggleRow({ title, description, checked, disabled = false, onChange }: ToggleRowProps) {
  return (
    <label className="toggle-row">
      <span>
        <strong>{title}</strong>
        <small>{description}</small>
      </span>

      <input
        type="checkbox"
        checked={checked}
        disabled={disabled}
        onChange={(event) => onChange(event.target.checked)}
      />
    </label>
  )
}