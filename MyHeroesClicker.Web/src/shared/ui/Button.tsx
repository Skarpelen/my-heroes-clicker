import clsx from 'clsx'
import type { ButtonHTMLAttributes, PropsWithChildren } from 'react'

type ButtonVariant = 'primary' | 'ghost' | 'danger'

type ButtonProps = PropsWithChildren<ButtonHTMLAttributes<HTMLButtonElement>> & {
  variant?: ButtonVariant
}

export function Button({ variant = 'primary', className, children, ...props }: ButtonProps) {
  return (
    <button className={clsx('button', `button-${variant}`, className)} {...props}>
      {children}
    </button>
  )
}